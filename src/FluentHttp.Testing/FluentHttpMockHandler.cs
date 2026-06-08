using System.Net;
using Fuzn.FluentHttp.Testing.Internals;

namespace Fuzn.FluentHttp.Testing;

/// <summary>
/// An <see cref="HttpMessageHandler"/> that intercepts requests and returns configured responses,
/// allowing FluentHttp-based code to be unit tested without making live HTTP calls.
/// Configure stubs with the <c>When*</c> methods, then build an <see cref="HttpClient"/> via
/// <see cref="CreateClient(string)"/> or <see cref="ToHttpClient"/>.
/// </summary>
public class FluentHttpMockHandler : HttpMessageHandler
{
    private readonly List<FluentHttpMockStub> _stubs = [];
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
    /// Gets the number of requests that did not match any stub.
    /// </summary>
    public int UnmatchedCount => Volatile.Read(ref _unmatchedCount);

    internal ISerializerProvider Serializer => _serializer;

    /// <summary>
    /// Configures a stub matching the given HTTP method and URL pattern.
    /// The pattern may be relative or absolute and may contain <c>*</c> wildcards.
    /// </summary>
    /// <param name="method">The HTTP method to match.</param>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A stub to configure matchers and the response.</returns>
    public FluentHttpMockStub When(HttpMethod method, string urlPattern)
    {
        ArgumentNullException.ThrowIfNull(method);
        return AddStub(method, urlPattern);
    }

    /// <summary>
    /// Configures a stub matching any HTTP method against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A stub to configure matchers and the response.</returns>
    public FluentHttpMockStub WhenAny(string urlPattern) => AddStub(null, urlPattern);

    /// <summary>
    /// Configures a stub matching an HTTP GET against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A stub to configure matchers and the response.</returns>
    public FluentHttpMockStub WhenGet(string urlPattern) => AddStub(HttpMethod.Get, urlPattern);

    /// <summary>
    /// Configures a stub matching an HTTP POST against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A stub to configure matchers and the response.</returns>
    public FluentHttpMockStub WhenPost(string urlPattern) => AddStub(HttpMethod.Post, urlPattern);

    /// <summary>
    /// Configures a stub matching an HTTP PUT against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A stub to configure matchers and the response.</returns>
    public FluentHttpMockStub WhenPut(string urlPattern) => AddStub(HttpMethod.Put, urlPattern);

    /// <summary>
    /// Configures a stub matching an HTTP PATCH against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A stub to configure matchers and the response.</returns>
    public FluentHttpMockStub WhenPatch(string urlPattern) => AddStub(HttpMethod.Patch, urlPattern);

    /// <summary>
    /// Configures a stub matching an HTTP DELETE against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A stub to configure matchers and the response.</returns>
    public FluentHttpMockStub WhenDelete(string urlPattern) => AddStub(HttpMethod.Delete, urlPattern);

    /// <summary>
    /// Sets the behavior used when an incoming request does not match any stub. Defaults to
    /// <see cref="MockFallbackBehavior.Throw"/>.
    /// </summary>
    /// <param name="behavior">The fallback behavior.</param>
    /// <returns>The current handler for method chaining.</returns>
    public FluentHttpMockHandler WithFallback(MockFallbackBehavior behavior)
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
    public FluentHttpMockHandler WithSerializer(ISerializerProvider serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        _serializer = serializer;
        return this;
    }

    /// <summary>
    /// Asserts that the given stub matched exactly the expected number of times.
    /// </summary>
    /// <param name="stub">The stub to verify.</param>
    /// <param name="expectedCount">The expected match count.</param>
    /// <exception cref="FluentHttpMockException">Thrown when the actual count differs.</exception>
    public void VerifyMatched(FluentHttpMockStub stub, int expectedCount)
    {
        ArgumentNullException.ThrowIfNull(stub);

        if (stub.MatchCount != expectedCount)
            throw new FluentHttpMockException(
                $"Expected stub to match {expectedCount} time(s), but it matched {stub.MatchCount} time(s).");
    }

    /// <summary>
    /// Asserts that every request received matched a stub.
    /// </summary>
    /// <exception cref="FluentHttpMockException">Thrown when one or more requests went unmatched.</exception>
    public void VerifyNoUnmatched()
    {
        if (UnmatchedCount > 0)
            throw new FluentHttpMockException(
                $"Expected all requests to match a stub, but {UnmatchedCount} request(s) went unmatched.");
    }

    /// <summary>
    /// Clears captured requests and unmatched count and resets each stub's match count.
    /// Configured stubs are retained.
    /// </summary>
    public void Reset()
    {
        lock (_gate)
        {
            _requests.Clear();
            foreach (var stub in _stubs)
                stub.ResetMatchCount();
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

        FluentHttpMockStub? matched;
        lock (_gate)
        {
            matched = _stubs.FirstOrDefault(s => s.Matches(request, body));
        }

        if (matched is not null)
            return await matched.CreateResponseAsync(request, cancellationToken);

        Interlocked.Increment(ref _unmatchedCount);

        if (_fallback == MockFallbackBehavior.RespondNotFound)
            return new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };

        throw new FluentHttpMockException(
            $"No stub matched the request: {request.Method} {request.RequestUri}. " +
            "Configure a stub with When*(), or use WithFallback(MockFallbackBehavior.RespondNotFound).");
    }

    private FluentHttpMockStub AddStub(HttpMethod? method, string urlPattern)
    {
        var stub = new FluentHttpMockStub(this, method, urlPattern);
        lock (_gate)
            _stubs.Add(stub);

        return stub;
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
