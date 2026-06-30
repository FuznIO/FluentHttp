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
    public int MatchCount => _matchCount;

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
    /// Requires the request body to equal the given object once serialized with the handler's serializer.
    /// </summary>
    /// <param name="body">The expected request body object.</param>
    /// <returns>The current rule for method chaining.</returns>
    public MockRule WithContent(object body)
    {
        ArgumentNullException.ThrowIfNull(body);

        _matchers.Add((_, actual) =>
        {
            var expected = _owner.Serializer.Serialize(body);
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
    public MockRule WithContentMatching(Func<string, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        _matchers.Add((_, actual) => predicate(actual ?? string.Empty));
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
    public MockRule WithDelay(TimeSpan delay)
    {
        _delay = delay;
        return this;
    }

    /// <summary>
    /// Responds with the given status code and, optionally, a JSON body. Starts a new response sequence,
    /// replacing any previously configured responses for this rule.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="jsonBody">An optional object serialized as JSON for the response body.</param>
    /// <returns>The current rule, for sequencing with <c>ThenRespondWith*</c>.</returns>
    public MockRule RespondWith(HttpStatusCode statusCode, object? jsonBody = null)
        => ConfigureStatus(NewPrimary(), statusCode, jsonBody);

    /// <summary>
    /// Responds with a JSON body serialized using the handler's serializer. Starts a new response sequence.
    /// </summary>
    /// <param name="body">The object to serialize as the response body.</param>
    /// <param name="statusCode">The HTTP status code to return. Defaults to 200 OK.</param>
    /// <returns>The current rule, for sequencing with <c>ThenRespondWith*</c>.</returns>
    public MockRule RespondWithJson(object body, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        ArgumentNullException.ThrowIfNull(body);
        return ConfigureStatus(NewPrimary(), statusCode, body);
    }

    /// <summary>
    /// Responds with a raw string body and the given content type. Starts a new response sequence.
    /// </summary>
    /// <param name="body">The response body.</param>
    /// <param name="contentType">The Content-Type for the body (e.g., "text/plain").</param>
    /// <param name="statusCode">The HTTP status code to return. Defaults to 200 OK.</param>
    /// <returns>The current rule, for sequencing with <c>ThenRespondWith*</c>.</returns>
    public MockRule RespondWithContent(string body, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK)
        => ConfigureContent(NewPrimary(), body, contentType, statusCode);

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
    /// Throws the given exception instead of returning a response, simulating a transport failure.
    /// Starts a new response sequence.
    /// </summary>
    /// <param name="exception">The exception to throw.</param>
    /// <returns>The current rule, for sequencing with <c>ThenRespondWith*</c>.</returns>
    public MockRule RespondWithException(Exception exception)
        => ConfigureException(NewPrimary(), exception);

    /// <summary>
    /// Simulates a timeout by cancelling the request (throwing <see cref="TaskCanceledException"/>).
    /// Combine with <see cref="WithDelay"/> to exercise a client-side timeout race. Starts a new response sequence.
    /// </summary>
    /// <returns>The current rule, for sequencing with <c>ThenRespondWith*</c>.</returns>
    public MockRule RespondWithTimeout()
        => ConfigureTimeout(NewPrimary());

    /// <summary>
    /// Adds the next response in the sequence: returned on the following matched request after the
    /// previously configured response(s). The last response in the sequence repeats once exhausted.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="jsonBody">An optional object serialized as JSON for the response body.</param>
    /// <returns>The current rule, for further sequencing.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called before a <c>RespondWith*</c> method.</exception>
    public MockRule ThenRespondWith(HttpStatusCode statusCode, object? jsonBody = null)
        => ConfigureStatus(NewNext(), statusCode, jsonBody);

    /// <summary>
    /// Adds the next response in the sequence with a JSON body. The last response repeats once exhausted.
    /// </summary>
    /// <param name="body">The object to serialize as the response body.</param>
    /// <param name="statusCode">The HTTP status code to return. Defaults to 200 OK.</param>
    /// <returns>The current rule, for further sequencing.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called before a <c>RespondWith*</c> method.</exception>
    public MockRule ThenRespondWithJson(object body, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        ArgumentNullException.ThrowIfNull(body);
        return ConfigureStatus(NewNext(), statusCode, body);
    }

    /// <summary>
    /// Adds the next response in the sequence with a raw string body. The last response repeats once exhausted.
    /// </summary>
    /// <param name="body">The response body.</param>
    /// <param name="contentType">The Content-Type for the body (e.g., "text/plain").</param>
    /// <param name="statusCode">The HTTP status code to return. Defaults to 200 OK.</param>
    /// <returns>The current rule, for further sequencing.</returns>
    /// <exception cref="InvalidOperationException">Thrown when called before a <c>RespondWith*</c> method.</exception>
    public MockRule ThenRespondWithContent(string body, string contentType, HttpStatusCode statusCode = HttpStatusCode.OK)
        => ConfigureContent(NewNext(), body, contentType, statusCode);

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

    internal void ResetMatchCount() => Interlocked.Exchange(ref _matchCount, 0);

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

    internal async Task<HttpResponseMessage> CreateResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var attempt = Interlocked.Increment(ref _matchCount);

        if (_delay > TimeSpan.Zero)
            await Task.Delay(_delay, cancellationToken);

        if (_responses.Count == 0)
            return BuildResponse(request, new ResponseSpec());

        // Advance through the sequence; the last response repeats once exhausted.
        var spec = _responses[Math.Min(attempt - 1, _responses.Count - 1)];

        switch (spec.Mode)
        {
            case ResponseMode.Exception:
                throw spec.Exception!;

            case ResponseMode.Timeout:
                throw new TaskCanceledException("The mocked request was configured to time out.");

            case ResponseMode.Custom:
                return spec.CustomResponse!;

            case ResponseMode.Factory:
                return spec.ResponseFactory!(request);

            case ResponseMode.Built:
            default:
                return BuildResponse(request, spec);
        }
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

    private MockRule ConfigureStatus(ResponseSpec spec, HttpStatusCode statusCode, object? jsonBody)
    {
        spec.Mode = ResponseMode.Built;
        spec.StatusCode = statusCode;
        spec.JsonBody = jsonBody;
        return this;
    }

    private MockRule ConfigureContent(ResponseSpec spec, string body, string contentType, HttpStatusCode statusCode)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        spec.Mode = ResponseMode.Built;
        spec.StatusCode = statusCode;
        spec.StringBody = body;
        spec.StringBodyContentType = contentType;
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

        if (spec.JsonBody is not null)
        {
            var json = _owner.Serializer.Serialize(spec.JsonBody);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        else if (spec.StringBody is not null)
        {
            response.Content = new StringContent(spec.StringBody, Encoding.UTF8, spec.StringBodyContentType!);
        }
        else
        {
            response.Content = new StringContent(string.Empty);
        }

        foreach (var header in _responseHeaders)
        {
            if (!response.Headers.TryAddWithoutValidation(header.Key, header.Value))
                response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return response;
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
        Exception,
        Timeout
    }

    private sealed class ResponseSpec
    {
        public ResponseMode Mode { get; set; } = ResponseMode.Built;
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public object? JsonBody { get; set; }
        public string? StringBody { get; set; }
        public string? StringBodyContentType { get; set; }
        public HttpResponseMessage? CustomResponse { get; set; }
        public Func<HttpRequestMessage, HttpResponseMessage>? ResponseFactory { get; set; }
        public Exception? Exception { get; set; }
    }
}
