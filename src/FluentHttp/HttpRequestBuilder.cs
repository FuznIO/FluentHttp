using Fuzn.FluentHttp.Internals;
using Fuzn.TestFuzn.Plugins.Http;
using System.Net;

namespace Fuzn.FluentHttp;

/// <summary>
/// Fluent builder for constructing and sending HTTP requests.
/// </summary>
public class HttpRequestBuilder
{
    private HttpClient _httpClient;
    private HttpRequestData _configuration = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpRequestBuilder"/> class.
    /// </summary>
    /// <param name="httpClient">The HttpClient instance to use for sending requests.</param>
    /// <param name="url">The target URL for the request.</param>
    internal HttpRequestBuilder(HttpClient httpClient, string url)
    {
        _httpClient = httpClient;

        if (_httpClient.BaseAddress == null)
            _configuration.Uri = new Uri(url);
        else
            _configuration.Uri = new Uri(_httpClient.BaseAddress, url);
    }

    /// <summary>
    /// Sets a custom serializer provider for request/response body serialization.
    /// </summary>
    /// <param name="serializerProvider">The serializer provider to use.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder SerializerProvider(ISerializerProvider serializerProvider)
    {
        _configuration.SerializerProvider = serializerProvider;
        return this;
    }

    /// <summary>
    /// Sets the Content-Type header for the request.
    /// </summary>
    /// <param name="contentType">The content type to use.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder ContentType(ContentTypes contentType)
    {
        _configuration.ContentType = contentType;
        return this;
    }

    /// <summary>
    /// Sets the request body. The body will be serialized based on the configured content type.
    /// </summary>
    /// <param name="body">The body content to send.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Body(object body)
    {
        _configuration.Body = body;
        return this;
    }

    /// <summary>
    /// Sets the Accept header for the request.
    /// </summary>
    /// <param name="acceptTypes">The accepted response content types.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Accept(AcceptTypes acceptTypes)
    {
        _configuration.AcceptTypes = acceptTypes;
        return this;
    }

    /// <summary>
    /// Adds a cookie to the request.
    /// </summary>
    /// <param name="cookie">The cookie to add.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Cookie(Cookie cookie)
    {
        _configuration.Cookies.Add(cookie);
        return this;
    }

    /// <summary>
    /// Adds a cookie to the request with the specified parameters.
    /// </summary>
    /// <param name="name">The name of the cookie.</param>
    /// <param name="value">The value of the cookie.</param>
    /// <param name="path">The path for which the cookie is valid. Defaults to null.</param>
    /// <param name="domain">The domain for which the cookie is valid. Defaults to null.</param>
    /// <param name="duration">The duration until the cookie expires. Defaults to 10 seconds if not specified.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Cookie(string name, string value, string? path = null, string? domain = null, TimeSpan? duration = null)
    {
        var cookie = new Cookie(name, value, path, domain);
        cookie.Expires = duration.HasValue ? DateTime.UtcNow.Add(duration.Value) : DateTime.UtcNow.AddSeconds(10);
        _configuration.Cookies.Add(cookie);
        return this;
    }

    /// <summary>
    /// Adds a single header to the request.
    /// </summary>
    /// <param name="key">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Header(string key, string value)
    {
        _configuration.Headers[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple headers to the request.
    /// </summary>
    /// <param name="headers">A dictionary of header names and values.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Headers(IDictionary<string, string> headers)
    {
        foreach (var header in headers)
            _configuration.Headers[header.Key] = header.Value;
        return this;
    }

    /// <summary>
    /// Sets Bearer token authentication for the request.
    /// </summary>
    /// <param name="token">The Bearer token.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when Basic authentication is already configured.</exception>
    public HttpRequestBuilder AuthBearer(string token)
    {
        if (!string.IsNullOrEmpty(_configuration.Auth.Basic))
            throw new InvalidOperationException("Cannot set both Bearer and Basic authentication.");
            
        _configuration.Auth = new Authentication { BearerToken = token };

        return this;
    }

    /// <summary>
    /// Sets Basic authentication for the request.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when Bearer authentication is already configured.</exception>
    public HttpRequestBuilder AuthBasic(string username, string password)
    {
        if (!string.IsNullOrEmpty(_configuration.Auth.BearerToken))
            throw new InvalidOperationException("Cannot set both Bearer and Basic authentication.");

        _configuration.Auth = new Authentication { Basic = BasicAuthenticationHelper.ToBase64String(username, password) };
        return this;
    }

    /// <summary>
    /// Registers an action to be invoked just before the request is sent.
    /// </summary>
    /// <param name="action">The action to execute with the constructed <see cref="HttpRequestMessage"/>.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder BeforeSend(Action<HttpRequestMessage> action)
    {
        _configuration.BeforeSend = action;
        return this;
    }

    ///// <summary>
    ///// Sets the logging verbosity level for the request.
    ///// </summary>
    ///// <param name="verbosity">The logging verbosity level.</param>
    ///// <returns>The current builder instance for method chaining.</returns>
    //public HttpRequestBuilder LoggingVerbosity(LoggingVerbosity verbosity)
    //{
    //    _loggingVerbosity = verbosity;
    //    return this;
    //}

    /// <summary>
    /// Sets a custom User-Agent header for the request.
    /// </summary>
    /// <param name="userAgent">The User-Agent string.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder UserAgent(string userAgent)
    {
        _configuration.UserAgent = userAgent;
        return this;
    }

    /// <summary>
    /// Sets the timeout for the request.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Timeout(TimeSpan timeout)
    {
        _configuration.Timeout = timeout;
        return this;
    }

    public HttpRequestBuilder Options(string key, object value)
    {
        _configuration.Options.Add(key, value);
        return this;
    }

    /// <summary>
    /// Sends the request using the HTTP GET method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Get()
    {
        _configuration.Method = HttpMethod.Get;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP POST method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Post()
    {
        _configuration.Method = HttpMethod.Post;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP PUT method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Put()
    {
        _configuration.Method = HttpMethod.Put;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP DELETE method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Delete()
    {
        _configuration.Method = HttpMethod.Delete;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP PATCH method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Patch()
    {
        _configuration.Method = HttpMethod.Patch;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP HEAD method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Head()
    {
        _configuration.Method = HttpMethod.Head;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP OPTIONS method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Options()
    {
        _configuration.Method = HttpMethod.Options;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP TRACE method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Trace()
    {
        _configuration.Method = HttpMethod.Trace;
        return Send();
    }
    
    internal async Task<HttpResponse> Send()
    {
        var request = _configuration.MapToHttpRequestMessage();

        var outputRequestResponse = false;
        HttpResponseMessage? response = null;
        string? responseBody = null;
        CookieContainer? responseCookies = null;

        try
        {
            var client = _httpClient;
            client.BaseAddress = _configuration.BaseUri;

            var cts = new CancellationTokenSource(_configuration.Timeout);


            if (request.Content != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync(cts.Token);
            }

            if (BeforeSend != null)
                _configuration.BeforeSend?.Invoke(request);

            response = await client.SendAsync(request, cts.Token);
            responseBody = await response.Content.ReadAsStringAsync(cts.Token);

            responseCookies = ExtractResponseCookies(response, _configuration.Uri);

            if (!response.IsSuccessStatusCode)
                outputRequestResponse = true;
        }
        catch (Exception ex)
        {
            outputRequestResponse = true;
            throw;
        }
        finally
        {
        }

        return new HttpResponse(request, response, responseCookies, body: responseBody, _configuration.SerializerProvider);
    }

    private static CookieContainer? ExtractResponseCookies(HttpResponseMessage response, Uri uri)
    {
        if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            var responseCookies = new CookieContainer();
            foreach (var setCookieHeader in setCookieHeaders)
            {
                try
                {
                    responseCookies.SetCookies(uri, setCookieHeader);
                }
                catch
                {
                    // Ignore malformed cookies
                }
            }

            return responseCookies;
        }

        return null;
    }
}
