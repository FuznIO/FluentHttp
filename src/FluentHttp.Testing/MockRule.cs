using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Fuzn.FluentHttp.Testing.Internals;

namespace Fuzn.FluentHttp.Testing;

/// <summary>
/// A single configured rule: a set of matchers describing which requests it handles, paired with
/// the response (or failure) it produces. Created via the <c>When*</c> methods on
/// <see cref="MockHttpHandler"/>.
/// </summary>
public sealed class MockRule
{
    private readonly MockHttpHandler _owner;
    private readonly HttpMethod? _method;
    private readonly UrlMatcher _urlMatcher;
    private readonly List<Func<HttpRequestMessage, string?, bool>> _matchers = [];
    private readonly Dictionary<string, string> _responseHeaders = new(StringComparer.OrdinalIgnoreCase);

    private readonly List<ResponseSpec> _responses = [];

    private TimeSpan _delay = TimeSpan.Zero;
    private int _matchCount;

    internal MockRule(MockHttpHandler owner, HttpMethod? method, string urlPattern)
    {
        ArgumentNullException.ThrowIfNull(owner);

        _owner = owner;
        _method = method;
        _urlMatcher = new UrlMatcher(urlPattern);
    }

    /// <summary>
    /// Gets the number of times this rule has matched an incoming request.
    /// </summary>
    public int MatchCount => Volatile.Read(ref _matchCount);

    /// <summary>
    /// Requires the request to contain the specified header, optionally with a specific value.
    /// </summary>
    /// <param name="name">The header name (case-insensitive).</param>
    /// <param name="value">The expected value, or <c>null</c> to match any value.</param>
    /// <returns>The current rule for method chaining.</returns>
    public MockRule WithHeader(string name, string? value = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _matchers.Add((request, _) =>
        {
            if (!TryGetHeaderValues(request, name, out var values))
                return false;

            return value is null || values.Contains(value, StringComparer.Ordinal);
        });

        return this;
    }

    /// <summary>
    /// Requires the request to contain the specified header with at least one value satisfying the predicate.
    /// </summary>
    /// <param name="name">The header name (case-insensitive).</param>
    /// <param name="valuePredicate">The predicate a header value must satisfy.</param>
    /// <returns>The current rule for method chaining.</returns>
    public MockRule WithHeader(string name, Func<string, bool> valuePredicate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(valuePredicate);

        _matchers.Add((request, _) =>
            TryGetHeaderValues(request, name, out var values) && values.Any(valuePredicate));

        return this;
    }

    /// <summary>
    /// Requires the request URL to contain the specified query parameter, optionally with a specific value.
    /// </summary>
    /// <param name="name">The query parameter name.</param>
    /// <param name="value">The expected value, or <c>null</c> to match any value.</param>
    /// <returns>The current rule for method chaining.</returns>
    public MockRule WithQueryParam(string name, string? value = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _matchers.Add((request, _) =>
        {
            var query = HttpUtility.ParseQueryString(request.RequestUri?.Query ?? string.Empty);
            if (!query.AllKeys.Contains(name))
                return false;

            return value is null || string.Equals(query[name], value, StringComparison.Ordinal);
        });

        return this;
    }

    /// <summary>
    /// Requires the request URL to contain the specified query parameter with a value satisfying the predicate.
    /// </summary>
    /// <param name="name">The query parameter name.</param>
    /// <param name="valuePredicate">The predicate the query parameter value must satisfy.</param>
    /// <returns>The current rule for method chaining.</returns>
    public MockRule WithQueryParam(string name, Func<string, bool> valuePredicate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(valuePredicate);

        _matchers.Add((request, _) =>
        {
            var query = HttpUtility.ParseQueryString(request.RequestUri?.Query ?? string.Empty);
            return query.AllKeys.Contains(name) && valuePredicate(query[name] ?? string.Empty);
        });

        return this;
    }

    /// <summary>
    /// Requires the request body to equal the specified string exactly.
    /// </summary>
    /// <param name="body">The expected request body.</param>
    /// <returns>The current rule for method chaining.</returns>
    public MockRule WithContent(string body)
    {
        ArgumentNullException.ThrowIfNull(body);

        _matchers.Add((_, actual) => string.Equals(actual, body, StringComparison.Ordinal));
        return this;
    }

    /// <summary>
    /// Requires the request body to equal the given object once serialized with the serializer FluentHttp
    /// resolves for the request's Content-Type.
    /// </summary>
    /// <param name="body">The expected request body object.</param>
    /// <returns>The current rule for method chaining.</returns>
    public MockRule WithContent(object body)
    {
        ArgumentNullException.ThrowIfNull(body);

        _matchers.Add((request, actual) =>
        {
            var contentType = request.Content?.Headers.ContentType?.ToString();
            var expected = _owner.ResolveSerializer(contentType).Serialize(body);
            return string.Equals(actual, expected, StringComparison.Ordinal);
        });

        return this;
    }

    /// <summary>
    /// Requires the request body to satisfy a custom predicate. The predicate receives the body as a
    /// string (empty string when there is no body).
    /// </summary>
    /// <param name="predicate">The predicate the body must satisfy.</param>
    /// <returns>The current rule for method chaining.</returns>
    public MockRule WithContent(Func<string, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _matchers.Add((_, actual) => predicate(actual ?? string.Empty));
        return this;
    }

    /// <summary>
    /// Requires the request to satisfy a custom predicate. Use this as an escape hatch for matching logic
    /// the typed matchers do not cover (for example, combining several headers with the path or method).
    /// </summary>
    /// <param name="predicate">The predicate the request must satisfy.</param>
    /// <returns>The current rule for method chaining.</returns>
    public MockRule WithRequest(Func<HttpRequestMessage, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _matchers.Add((request, _) => predicate(request));
        return this;
    }

    /// <summary>
    /// Adds a header to the response this rule produces.
    /// </summary>
    /// <param name="name">The response header name.</param>
    /// <param name="value">The response header value.</param>
    /// <returns>The current rule for method chaining.</returns>
    public MockRule WithResponseHeader(string name, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);

        _responseHeaders[name] = value;
        return this;
    }

    /// <summary>
    /// Delays the response by the specified duration, honoring the request's cancellation token.
    /// Useful for exercising client-side timeouts.
    /// </summary>
    /// <param name="delay">The delay before the response is produced.</param>
    /// <returns>The current rule for method chaining.</returns>
    public MockRule WithResponseDelay(TimeSpan delay)
    {
        _delay = delay;
        return this;
    }

    /// <summary>
    /// Responds with the given status code and no body. Starts a new response sequence, replacing any
    /// previously configured responses for this rule.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <returns>The current rule, for sequencing with <c>ThenRespondWith*</c>.</returns>
    public MockRule RespondWith(HttpStatusCode statusCode)
        => ConfigureBuilt(NewPrimary(), statusCode, body: null, contentType: null);

    /// <summary>
    /// Responds with a body and content type. Mirrors <c>WithContent</c> on the request side: a string body
    /// is sent as-is, and any other object is serialized with the serializer registered for the content type.
    /// <paramref name="contentType"/> defaults to <c>application/json</c>. Starts a new response sequence.
    /// </summary>
    /// <param name="body">The response body - a string is sent raw; any other object is serialized.</param>
    /// <param name="contentType">The Content-Type for the body. Defaults to <c>application/json</c>.</param>
    /// <param name="statusCode">The HTTP status code to return. Defaults to 200 OK.</param>
    /// <returns>The current rule, for sequencing with <c>ThenRespondWith*</c>.</returns>
    public MockRule RespondWithContent(object body, string? contentType = null, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        ArgumentNullException.ThrowIfNull(body);
        return ConfigureBuilt(NewPrimary(), statusCode, body, contentType);
    }

    /// <summary>
    /// Responds with a fully custom <see cref="HttpResponseMessage"/>. Starts a new response sequence.
    /// </summary>
    /// <param name="response">The response to return.</param>
    /// <returns>The current rule, for sequencing with <c>ThenRespondWith*</c>.</returns>
    public MockRule RespondWith(HttpResponseMessage response)
        => ConfigureCustom(NewPrimary(), response);

    /// <summary>
    /// Responds using a factory that builds an <see cref="HttpResponseMessage"/> from the incoming request.
    /// Starts a new response sequence.
    /// </summary>
    /// <param name="responseFactory">The factory invoked for each matched request.</param>
    /// <returns>The current rule, for sequencing with <c>ThenRespondWith*</c>.</returns>
    public MockRule RespondWith(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        => ConfigureFactory(NewPrimary(), responseFactory);

    /// <summary>
    /// Responds using an asynchronous factory that builds an <see cref="HttpResponseMessage"/> from the
    /// incoming request. Starts a new response sequence.
    /// </summary>
    /// <param name="responseFactory">The asynchronous factory invoked for each matched request.</param>
    /// <returns>The current rule, for sequencing with <c>ThenRespondWith*</c>.</returns>
    public MockRule RespondWith(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
        => ConfigureAsyncFactory(NewPrimary(), responseFactory);

    /// <summary>
    /// Throws the given exception instead of returning a response, simulating a transport failure.
    /// Starts a new response sequence.
    /// </summary>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The current rule, for sequencing with <c>ThenRespondWith*</c>.</returns>
    public MockRule RespondWithException(Exception exception)
        => ConfigureException(NewPrimary(), exception);

    /// <summary>
    /// Simulates a timeout by cancelling the request (throwing <see cref="TaskCanceledException"/>).
    /// Combine with <see cref="WithResponseDelay"/> to exercise a client-side timeout race. Starts a new response sequence.
    /// </summary>
    /// <returns>The current rule, for sequencing with <c>ThenRespondWith*</c>.</returns>
    public MockRule RespondWithTimeout()
        => ConfigureTimeout(NewPrimary());

    /// <summary>
    /// Adds the next response in the sequence with the given status code and no body. The last response in
    /// the sequence repeats once exhausted.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <returns>The current rule, for further sequencing.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called before a <c>RespondWith*</c> method.</exception>
    public MockRule ThenRespondWith(HttpStatusCode statusCode)
        => ConfigureBuilt(NewNext(), statusCode, body: null, contentType: null);

    /// <summary>
    /// Adds the next response in the sequence with a body and content type (see <see cref="RespondWithContent"/>).
    /// The last response repeats once exhausted.
    /// </summary>
    /// <param name="body">The response body - a string is sent raw; any other object is serialized.</param>
    /// <param name="contentType">The Content-Type for the body. Defaults to <c>application/json</c>.</param>
    /// <param name="statusCode">The HTTP status code to return. Defaults to 200 OK.</param>
    /// <returns>The current rule, for further sequencing.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called before a <c>RespondWith*</c> method.</exception>
    public MockRule ThenRespondWithContent(object body, string? contentType = null, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        ArgumentNullException.ThrowIfNull(body);
        return ConfigureBuilt(NewNext(), statusCode, body, contentType);
    }

    /// <summary>
    /// Adds the next response in the sequence as a fully custom <see cref="HttpResponseMessage"/>.
    /// The last response repeats once exhausted.
    /// </summary>
    /// <param name="response">The response to return.</param>
    /// <returns>The current rule, for further sequencing.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called before a <c>RespondWith*</c> method.</exception>
    public MockRule ThenRespondWith(HttpResponseMessage response)
        => ConfigureCustom(NewNext(), response);

    /// <summary>
    /// Adds the next response in the sequence, built by a factory from the incoming request.
    /// The last response repeats once exhausted.
    /// </summary>
    /// <param name="responseFactory">The factory invoked for the matched request.</param>
    /// <returns>The current rule, for further sequencing.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called before a <c>RespondWith*</c> method.</exception>
    public MockRule ThenRespondWith(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        => ConfigureFactory(NewNext(), responseFactory);

    /// <summary>
    /// Adds the next response in the sequence, built by an asynchronous factory from the incoming request.
    /// The last response repeats once exhausted.
    /// </summary>
    /// <param name="responseFactory">The asynchronous factory invoked for the matched request.</param>
    /// <returns>The current rule, for further sequencing.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called before a <c>RespondWith*</c> method.</exception>
    public MockRule ThenRespondWith(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
        => ConfigureAsyncFactory(NewNext(), responseFactory);

    /// <summary>
    /// Adds a transport failure as the next response in the sequence. The last response repeats once exhausted.
    /// </summary>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The current rule, for further sequencing.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called before a <c>RespondWith*</c> method.</exception>
    public MockRule ThenRespondWithException(Exception exception)
        => ConfigureException(NewNext(), exception);

    /// <summary>
    /// Adds a timeout as the next response in the sequence. The last response repeats once exhausted.
    /// </summary>
    /// <returns>The current rule, for further sequencing.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called before a <c>RespondWith*</c> method.</exception>
    public MockRule ThenRespondWithTimeout()
        => ConfigureTimeout(NewNext());


    internal bool Matches(HttpRequestMessage request, string? body)
    {
        if (_method is not null && request.Method != _method)
            return false;

        if (request.RequestUri is null || !_urlMatcher.IsMatch(request.RequestUri))
            return false;

        foreach (var matcher in _matchers)
        {
            if (!matcher(request, body))
                return false;
        }

        return true;
    }

    internal async Task<HttpResponseMessage> CreateResponse(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var attempt = Interlocked.Increment(ref _matchCount);

        if (_delay > TimeSpan.Zero)
            await Task.Delay(_delay, cancellationToken);

        // Advance through the sequence; the last response repeats once exhausted.
        var spec = _responses.Count == 0
            ? new ResponseSpec()
            : _responses[Math.Min(attempt - 1, _responses.Count - 1)];

        var response = spec.Mode switch
        {
            ResponseMode.Exception => throw spec.Exception!,
            ResponseMode.Timeout => throw new TaskCanceledException("The mocked request was configured to time out."),
            ResponseMode.Custom => await CloneResponse(spec, request, cancellationToken),
            ResponseMode.Factory => spec.ResponseFactory!(request),
            ResponseMode.AsyncFactory => await spec.AsyncResponseFactory!(request, cancellationToken),
            _ => BuildResponse(request, spec),
        };

        ApplyResponseHeaders(response);
        return response;
    }

    private ResponseSpec NewPrimary()
    {
        _responses.Clear();
        var spec = new ResponseSpec();
        _responses.Add(spec);
        return spec;
    }

    private ResponseSpec NewNext()
    {
        if (_responses.Count == 0)
            throw new InvalidOperationException("Call a RespondWith* method before ThenRespondWith* to start a response sequence.");

        var spec = new ResponseSpec();
        _responses.Add(spec);
        return spec;
    }

    private MockRule ConfigureBuilt(ResponseSpec spec, HttpStatusCode statusCode, object? body, string? contentType)
    {
        spec.Mode = ResponseMode.Built;
        spec.StatusCode = statusCode;
        spec.Body = body;
        spec.BodyContentType = contentType;
        return this;
    }

    private MockRule ConfigureCustom(ResponseSpec spec, HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);

        spec.Mode = ResponseMode.Custom;
        spec.CustomResponse = response;
        return this;
    }

    private MockRule ConfigureFactory(ResponseSpec spec, Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        ArgumentNullException.ThrowIfNull(responseFactory);

        spec.Mode = ResponseMode.Factory;
        spec.ResponseFactory = responseFactory;
        return this;
    }

    private MockRule ConfigureAsyncFactory(ResponseSpec spec, Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory)
    {
        ArgumentNullException.ThrowIfNull(responseFactory);

        spec.Mode = ResponseMode.AsyncFactory;
        spec.AsyncResponseFactory = responseFactory;
        return this;
    }

    private MockRule ConfigureException(ResponseSpec spec, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        spec.Mode = ResponseMode.Exception;
        spec.Exception = exception;
        return this;
    }

    private MockRule ConfigureTimeout(ResponseSpec spec)
    {
        spec.Mode = ResponseMode.Timeout;
        return this;
    }

    private HttpResponseMessage BuildResponse(HttpRequestMessage request, ResponseSpec spec)
    {
        var response = new HttpResponseMessage(spec.StatusCode) { RequestMessage = request };

        if (spec.Body is null)
        {
            response.Content = new StringContent(string.Empty);
            return response;
        }

        // Mirror FluentHttp's WithContent: a string is sent as-is; any other object is serialized with
        // the serializer registered for the content type (defaulting to application/json).
        var contentType = spec.BodyContentType ?? "application/json";
        var body = spec.Body is string raw
            ? raw
            : _owner.ResolveSerializer(contentType).Serialize(spec.Body);

        response.Content = new StringContent(body, Encoding.UTF8, contentType);
        return response;
    }

    private void ApplyResponseHeaders(HttpResponseMessage response)
    {
        foreach (var header in _responseHeaders)
        {
            if (!response.Headers.TryAddWithoutValidation(header.Key, header.Value))
                response.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    private static async Task<HttpResponseMessage> CloneResponse(ResponseSpec spec, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var source = spec.CustomResponse!;

        var clone = new HttpResponseMessage(source.StatusCode)
        {
            RequestMessage = request,
            ReasonPhrase = source.ReasonPhrase,
            Version = source.Version,
        };

        if (source.Content is not null)
        {
            // Buffer the source body once so each matched request gets an independent, re-readable copy.
            // A single HttpResponseMessage instance would otherwise be disposed/consumed after the first match.
            spec.CustomBodyBytes ??= await source.Content.ReadAsByteArrayAsync(cancellationToken);

            var content = new ByteArrayContent(spec.CustomBodyBytes);
            foreach (var header in source.Content.Headers)
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);

            clone.Content = content;
        }

        foreach (var header in source.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }

    private static bool TryGetHeaderValues(HttpRequestMessage request, string name, out IEnumerable<string> values)
    {
        if (request.Headers.TryGetValues(name, out var headerValues))
        {
            values = headerValues;
            return true;
        }

        if (request.Content is not null && request.Content.Headers.TryGetValues(name, out var contentValues))
        {
            values = contentValues;
            return true;
        }

        values = [];
        return false;
    }

    private enum ResponseMode
    {
        Built,
        Custom,
        Factory,
        AsyncFactory,
        Exception,
        Timeout
    }

    private sealed class ResponseSpec
    {
        public ResponseMode Mode { get; set; } = ResponseMode.Built;
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public object? Body { get; set; }
        public string? BodyContentType { get; set; }
        public HttpResponseMessage? CustomResponse { get; set; }
        public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFactory { get; set; }
        public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? AsyncResponseFactory { get; set; }
        public byte[]? CustomBodyBytes { get; set; }
        public Exception? Exception { get; set; }
    }
}
