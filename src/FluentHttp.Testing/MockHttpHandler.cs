using System.Net;
using Fuzn.FluentHttp.Testing.Internals;

namespace Fuzn.FluentHttp.Testing;

/// <summary>
/// An <see cref="HttpMessageHandler"/> that intercepts requests and returns configured responses,
/// allowing FluentHttp-based code to be unit tested without making live HTTP calls.
/// Configure rules with the <c>When*</c> methods, then build an <see cref="HttpClient"/> via
/// <see cref="CreateClient(string)"/> or <see cref="ToHttpClient"/>.
/// </summary>
public class MockHttpHandler : HttpMessageHandler
{
    private readonly List<MockRule> _rules = [];
    private readonly List<CapturedRequest> _requests = [];
    private readonly Lock _gate = new();

    private MockFallbackBehavior _fallback = MockFallbackBehavior.Throw;
    private ISerializerProvider _serializer = FluentHttpDefaults.Serializers.Default;
    private int _unmatchedCount;

    /// <summary>
    /// Gets the requests captured by this handler, in the order they were received.
    /// </summary>
    public IReadOnlyList<CapturedRequest> Requests
    {
        get
        {
            lock (_gate)
                return _requests.ToArray();
        }
    }

    /// <summary>
    /// Gets the number of requests that did not match any rule.
    /// </summary>
    public int UnmatchedCount => Volatile.Read(ref _unmatchedCount);

    internal ISerializerProvider Serializer => _serializer;

    /// <summary>
    /// Configures a rule matching the given HTTP method and URL pattern.
    /// The pattern may be relative or absolute and may contain <c>*</c> wildcards.
    /// </summary>
    /// <param name="method">The HTTP method to match.</param>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule When(HttpMethod method, string urlPattern)
    {
        ArgumentNullException.ThrowIfNull(method);
        return AddRule(method, urlPattern);
    }

    /// <summary>
    /// Configures a rule matching any HTTP method against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenAny(string urlPattern) => AddRule(null, urlPattern);

    /// <summary>
    /// Configures a rule matching an HTTP GET against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenGet(string urlPattern) => AddRule(HttpMethod.Get, urlPattern);

    /// <summary>
    /// Configures a rule matching an HTTP POST against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenPost(string urlPattern) => AddRule(HttpMethod.Post, urlPattern);

    /// <summary>
    /// Configures a rule matching an HTTP PUT against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenPut(string urlPattern) => AddRule(HttpMethod.Put, urlPattern);

    /// <summary>
    /// Configures a rule matching an HTTP PATCH against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenPatch(string urlPattern) => AddRule(HttpMethod.Patch, urlPattern);

    /// <summary>
    /// Configures a rule matching an HTTP DELETE against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenDelete(string urlPattern) => AddRule(HttpMethod.Delete, urlPattern);

    /// <summary>
    /// Sets the behavior used when an incoming request does not match any rule. Defaults to
    /// <see cref="MockFallbackBehavior.Throw"/>.
    /// </summary>
    /// <param name="behavior">The fallback behavior.</param>
    /// <returns>The current handler for method chaining.</returns>
    public MockHttpHandler WithFallback(MockFallbackBehavior behavior)
    {
        _fallback = behavior;
        return this;
    }

    /// <summary>
    /// Overrides the serializer used to serialize response bodies and to match/deserialize request bodies.
    /// Defaults to <see cref="FluentHttpDefaults.Serializers"/>' default serializer.
    /// </summary>
    /// <param name="serializer">The serializer provider to use.</param>
    /// <returns>The current handler for method chaining.</returns>
    public MockHttpHandler WithSerializer(ISerializerProvider serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        _serializer = serializer;
        return this;
    }

    /// <summary>
    /// Asserts that the given rule matched exactly the expected number of times.
    /// </summary>
    /// <param name="rule">The rule to verify.</param>
    /// <param name="expectedCount">The expected match count.</param>
    /// <exception cref="MockHttpException">Thrown when the actual count differs.</exception>
    public void VerifyMatched(MockRule rule, int expectedCount)
    {
        ArgumentNullException.ThrowIfNull(rule);

        if (rule.MatchCount != expectedCount)
            throw new MockHttpException(
                $"Expected rule to match {expectedCount} time(s), but it matched {rule.MatchCount} time(s).");
    }

    /// <summary>
    /// Asserts that every request received matched a rule.
    /// </summary>
    /// <exception cref="MockHttpException">Thrown when one or more requests went unmatched.</exception>
    public void VerifyNoUnmatched()
    {
        if (UnmatchedCount > 0)
            throw new MockHttpException(
                $"Expected all requests to match a rule, but {UnmatchedCount} request(s) went unmatched.");
    }

    /// <summary>
    /// Clears captured requests and unmatched count and resets each rule's match count.
    /// Configured rules are retained.
    /// </summary>
    public void Reset()
    {
        lock (_gate)
        {
            _requests.Clear();
            foreach (var rule in _rules)
                rule.ResetMatchCount();
        }

        Volatile.Write(ref _unmatchedCount, 0);
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> backed by this handler with the given base address.
    /// </summary>
    /// <param name="baseAddress">The base address to assign to the client.</param>
    /// <returns>A configured <see cref="HttpClient"/>.</returns>
    public HttpClient CreateClient(string baseAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseAddress);
        return CreateClient(new Uri(baseAddress));
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> backed by this handler with the given base address.
    /// </summary>
    /// <param name="baseAddress">The base address to assign to the client.</param>
    /// <returns>A configured <see cref="HttpClient"/>.</returns>
    public HttpClient CreateClient(Uri baseAddress)
    {
        ArgumentNullException.ThrowIfNull(baseAddress);
        return new HttpClient(this, disposeHandler: false) { BaseAddress = baseAddress };
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> backed by this handler with no base address.
    /// Use absolute URLs with the returned client.
    /// </summary>
    /// <returns>A configured <see cref="HttpClient"/>.</returns>
    public HttpClient ToHttpClient() => new(this, disposeHandler: false);

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var body = await RequestBodyReader.ReadAsStringAsync(request.Content, cancellationToken);
        Capture(request, body);

        MockRule? matched;
        lock (_gate)
        {
            matched = _rules.FirstOrDefault(s => s.Matches(request, body));
        }

        if (matched is not null)
            return await matched.CreateResponseAsync(request, cancellationToken);

        Interlocked.Increment(ref _unmatchedCount);

        if (_fallback == MockFallbackBehavior.RespondNotFound)
            return new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };

        throw new MockHttpException(
            $"No rule matched the request: {request.Method} {request.RequestUri}. " +
            "Configure a rule with When*(), or use WithFallback(MockFallbackBehavior.RespondNotFound).");
    }

    private MockRule AddRule(HttpMethod? method, string urlPattern)
    {
        var rule = new MockRule(this, method, urlPattern);
        lock (_gate)
            _rules.Add(rule);

        return rule;
    }

    private void Capture(HttpRequestMessage request, string? body)
    {
        var headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in request.Headers)
            headers[header.Key] = header.Value.ToArray();

        if (request.Content is not null)
        {
            foreach (var header in request.Content.Headers)
                headers[header.Key] = header.Value.ToArray();
        }

        var captured = new CapturedRequest(
            request.Method,
            request.RequestUri!,
            headers,
            body,
            request.Content?.Headers.ContentType?.ToString(),
            _serializer);

        lock (_gate)
            _requests.Add(captured);
    }
}
