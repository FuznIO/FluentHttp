using System.Net;
using Fuzn.FluentHttp.Testing.Internals;

namespace Fuzn.FluentHttp.Testing;

/// <summary>
/// An <see cref="HttpMessageHandler"/> that intercepts requests and returns configured responses,
/// allowing FluentHttp-based code to be unit tested without making live HTTP calls.
/// Configure rules with the <c>When*</c> methods, then build an <see cref="HttpClient"/> via
/// <see cref="CreateClient(string)"/> (or <see cref="CreateClient()"/> for absolute URLs).
/// </summary>
public class MockHttpHandler : HttpMessageHandler
{
    private readonly List<MockRule> _rules = [];
    private readonly List<CapturedRequest> _requests = [];
    private readonly Lock _gate = new();
    private ISerializerProvider? _serializerOverride;

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
    /// Resolves the serializer for a body with the given content type, the same way FluentHttp does:
    /// an explicit <see cref="WithSerializer"/> override wins; otherwise the global
    /// <see cref="FluentHttpDefaults.Serializers"/> registry is resolved by content type, falling back to its default.
    /// Resolution happens per call so it reflects the serializer configuration in effect at request time.
    /// </summary>
    internal ISerializerProvider ResolveSerializer(string? contentType)
        => _serializerOverride ?? FluentHttpDefaults.Serializers.ResolveOrDefault(contentType);

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
    /// Configures a rule matching the given HTTP method against any URL.
    /// </summary>
    /// <param name="method">The HTTP method to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule When(HttpMethod method)
    {
        ArgumentNullException.ThrowIfNull(method);
        return AddRule(method, "*");
    }

    /// <summary>
    /// Configures a rule matching any HTTP method against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenAny(string urlPattern) => AddRule(null, urlPattern);

    /// <summary>
    /// Configures a rule matching any HTTP method against any URL. Useful when the test exercises a
    /// single endpoint and the exact method and URL do not matter.
    /// </summary>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenAny() => AddRule(null, "*");

    /// <summary>
    /// Configures a rule matching an HTTP GET against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenGet(string urlPattern) => AddRule(HttpMethod.Get, urlPattern);

    /// <summary>
    /// Configures a rule matching an HTTP GET against any URL.
    /// </summary>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenGet() => AddRule(HttpMethod.Get, "*");

    /// <summary>
    /// Configures a rule matching an HTTP POST against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenPost(string urlPattern) => AddRule(HttpMethod.Post, urlPattern);

    /// <summary>
    /// Configures a rule matching an HTTP POST against any URL.
    /// </summary>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenPost() => AddRule(HttpMethod.Post, "*");

    /// <summary>
    /// Configures a rule matching an HTTP PUT against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenPut(string urlPattern) => AddRule(HttpMethod.Put, urlPattern);

    /// <summary>
    /// Configures a rule matching an HTTP PUT against any URL.
    /// </summary>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenPut() => AddRule(HttpMethod.Put, "*");

    /// <summary>
    /// Configures a rule matching an HTTP PATCH against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenPatch(string urlPattern) => AddRule(HttpMethod.Patch, urlPattern);

    /// <summary>
    /// Configures a rule matching an HTTP PATCH against any URL.
    /// </summary>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenPatch() => AddRule(HttpMethod.Patch, "*");

    /// <summary>
    /// Configures a rule matching an HTTP DELETE against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenDelete(string urlPattern) => AddRule(HttpMethod.Delete, urlPattern);

    /// <summary>
    /// Configures a rule matching an HTTP DELETE against any URL.
    /// </summary>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenDelete() => AddRule(HttpMethod.Delete, "*");

    /// <summary>
    /// Configures a rule matching an HTTP HEAD against the given URL pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenHead(string urlPattern) => AddRule(HttpMethod.Head, urlPattern);

    /// <summary>
    /// Configures a rule matching an HTTP HEAD against any URL.
    /// </summary>
    /// <returns>A rule to configure matchers and the response.</returns>
    public MockRule WhenHead() => AddRule(HttpMethod.Head, "*");

    /// <summary>
    /// Overrides serializer resolution with an explicit serializer for all bodies. By default the handler
    /// resolves the serializer the same way FluentHttp does - by the message's Content-Type against the global
    /// <see cref="FluentHttpDefaults.Serializers"/> registry, falling back to its default. Set this only when
    /// the code under test uses a per-request serializer the handler cannot otherwise see.
    /// </summary>
    /// <param name="serializer">The serializer provider to use.</param>
    /// <returns>The current handler for method chaining.</returns>
    public MockHttpHandler WithSerializer(ISerializerProvider serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        _serializerOverride = serializer;
        return this;
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> backed by this handler with no base address.
    /// Use absolute URLs with the returned client, or when the code under test sets its own base address.
    /// </summary>
    /// <returns>A configured <see cref="HttpClient"/>.</returns>
    public HttpClient CreateClient() => new(this, disposeHandler: false);

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

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var bodyBytes = await RequestBodyReader.ReadAsByteArray(request.Content, cancellationToken);
        var body = bodyBytes is null ? null : RequestBodyReader.Decode(request.Content, bodyBytes);
        Capture(request, body, bodyBytes);

        MockRule? matched;
        lock (_gate)
        {
            matched = _rules.FirstOrDefault(s => s.Matches(request, body));
        }

        if (matched is not null)
            return await matched.CreateResponse(request, cancellationToken);

        throw new MockHttpException(
            $"No rule matched the request: {request.Method} {request.RequestUri}. " +
            "Configure a rule with When*(). To return a response for otherwise-unmatched requests, " +
            "register a catch-all rule last, e.g. WhenAny().RespondWith(HttpStatusCode.NotFound).");
    }

    private MockRule AddRule(HttpMethod? method, string urlPattern)
    {
        var rule = new MockRule(this, method, urlPattern);
        lock (_gate)
            _rules.Add(rule);

        return rule;
    }

    private void Capture(HttpRequestMessage request, string? body, byte[]? bodyBytes)
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
            bodyBytes,
            request.Content?.Headers.ContentType?.ToString(),
            ResolveSerializer);

        lock (_gate)
            _requests.Add(captured);
    }
}
