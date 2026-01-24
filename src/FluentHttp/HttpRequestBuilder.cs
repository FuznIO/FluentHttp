using Fuzn.FluentHttp.Internals;
using System.Net;

namespace Fuzn.FluentHttp;

/// <summary>
/// Fluent builder for constructing and sending HTTP requests.
/// </summary>
public class HttpRequestBuilder
{
    private HttpClient _httpClient;
    private HttpRequestData _data = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpRequestBuilder"/> class.
    /// </summary>
    /// <param name="httpClient">The HttpClient instance to use for sending requests.</param>
    /// <param name="url">The target URL for the request.</param>
    internal HttpRequestBuilder(HttpClient httpClient, string url)
    {
        if (httpClient == null)
            throw new ArgumentNullException(nameof(httpClient));

        if (string.IsNullOrEmpty(url))
            throw new ArgumentNullException(nameof(url));

        _httpClient = httpClient;
        

        if (_httpClient.BaseAddress == null)
        {
            _data.Uri = new Uri(url);
            _httpClient.BaseAddress = _data.BaseUri;
            
        }
        else
        {
            _data.Uri = new Uri(_httpClient.BaseAddress, url);
        }

            var client = _httpClient;
            client.BaseAddress = _data.BaseUri;
    }

    /// <summary>
    /// Sets a custom serializer provider for request/response body serialization.
    /// </summary>
    /// <param name="serializerProvider">The serializer provider to use.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder SerializerProvider(ISerializerProvider serializerProvider)
    {
        _data.SerializerProvider = serializerProvider;
        return this;
    }

    /// <summary>
    /// Sets the Content-Type header for the request.
    /// </summary>
    /// <param name="contentType">The content type to use.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder ContentType(ContentTypes contentType)
    {
        _data.ContentType = contentType;
        return this;
    }

    /// <summary>
    /// Sets the request body. The body will be serialized based on the configured content type.
    /// </summary>
    /// <param name="body">The body content to send.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Body(object body)
    {
        _data.Body = body;
        return this;
    }

    /// <summary>
    /// Configures the request as multipart/form-data for file uploads.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder AsMultipart()
    {
        _data.ContentType = ContentTypes.Multipart;
        return this;
    }

    /// <summary>
    /// Adds a file to the request. Automatically sets the content type to multipart/form-data.
    /// </summary>
    /// <param name="name">The form field name for the file.</param>
    /// <param name="fileName">The file name to be sent in the Content-Disposition header.</param>
    /// <param name="content">The file content as a stream.</param>
    /// <param name="contentType">The MIME type of the file. Defaults to "application/octet-stream".</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder AttachFile(string name, string fileName, Stream content, string contentType = "application/octet-stream")
    {
        _data.ContentType = ContentTypes.Multipart;
        _data.Files.Add(new FileContent(name, fileName, content, contentType));
        return this;
    }

    /// <summary>
    /// Adds a file to the request. Automatically sets the content type to multipart/form-data.
    /// </summary>
    /// <param name="name">The form field name for the file.</param>
    /// <param name="fileName">The file name to be sent in the Content-Disposition header.</param>
    /// <param name="content">The file content as a byte array.</param>
    /// <param name="contentType">The MIME type of the file. Defaults to "application/octet-stream".</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder AttachFile(string name, string fileName, byte[] content, string contentType = "application/octet-stream")
    {
        _data.ContentType = ContentTypes.Multipart;
        _data.Files.Add(new FileContent(name, fileName, content, contentType));
        return this;
    }

    /// <summary>
    /// Adds a file to the request using a FileContent object. Automatically sets the content type to multipart/form-data.
    /// </summary>
    /// <param name="file">The file content to attach.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder AttachFile(FileContent file)
    {
        _data.ContentType = ContentTypes.Multipart;
        _data.Files.Add(file);
        return this;
    }

    /// <summary>
    /// Adds a form field to the multipart request. Automatically sets the content type to multipart/form-data.
    /// </summary>
    /// <param name="name">The form field name.</param>
    /// <param name="value">The form field value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder FormField(string name, string value)
    {
        _data.ContentType = ContentTypes.Multipart;
        _data.FormFields[name] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple form fields to the multipart request. Automatically sets the content type to multipart/form-data.
    /// </summary>
    /// <param name="fields">A dictionary of form field names and values.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder FormFields(IDictionary<string, string> fields)
    {
        _data.ContentType = ContentTypes.Multipart;
        foreach (var field in fields)
            _data.FormFields[field.Key] = field.Value;
        return this;
    }

    /// <summary>
    /// Sets the Accept header for the request.
    /// </summary>
    /// <param name="acceptTypes">The accepted response content types.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Accept(AcceptTypes acceptTypes)
    {
        _data.AcceptTypes = acceptTypes;
        return this;
    }

    /// <summary>
    /// Adds a cookie to the request.
    /// </summary>
    /// <param name="cookie">The cookie to add.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Cookie(Cookie cookie)
    {
        _data.Cookies.Add(cookie);
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
        _data.Cookies.Add(cookie);
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
        _data.Headers[key] = value;
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
            _data.Headers[header.Key] = header.Value;
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
        if (!string.IsNullOrEmpty(_data.Auth.Basic))
            throw new InvalidOperationException("Cannot set both Bearer and Basic authentication.");
            
        _data.Auth = new Authentication { BearerToken = token };

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
        if (!string.IsNullOrEmpty(_data.Auth.BearerToken))
            throw new InvalidOperationException("Cannot set both Bearer and Basic authentication.");

        _data.Auth = new Authentication { Basic = BasicAuthenticationHelper.ToBase64String(username, password) };
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
        _data.UserAgent = userAgent;
        return this;
    }

    /// <summary>
    /// Sets the timeout for the request.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Timeout(TimeSpan timeout)
    {
        _data.Timeout = timeout;
        return this;
    }

    public HttpRequestBuilder Options(string key, object value)
    {
        _data.Options.Add(key, value);
        return this;
    }

    /// <summary>
    /// Sends the request using the HTTP GET method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Get()
    {
        _data.Method = HttpMethod.Get;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP POST method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Post()
    {
        _data.Method = HttpMethod.Post;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP PUT method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Put()
    {
        _data.Method = HttpMethod.Put;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP DELETE method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Delete()
    {
        _data.Method = HttpMethod.Delete;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP PATCH method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Patch()
    {
        _data.Method = HttpMethod.Patch;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP HEAD method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Head()
    {
        _data.Method = HttpMethod.Head;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP OPTIONS method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Options()
    {
        _data.Method = HttpMethod.Options;
        return Send();
    }

    /// <summary>
    /// Sends the request using the HTTP TRACE method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Trace()
    {
        _data.Method = HttpMethod.Trace;
        return Send();
    }

    /// <summary>
    /// Downloads the response as a stream using HTTP GET. 
    /// The returned HttpStreamResponse should be disposed after use.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the streaming response.</returns>
    public Task<HttpStreamResponse> GetStream(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Get;
        return SendForStream(cancellationToken);
    }

    /// <summary>
    /// Sends a POST request and returns the response as a stream.
    /// The returned HttpStreamResponse should be disposed after use.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the streaming response.</returns>
    public Task<HttpStreamResponse> PostStream(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Post;
        return SendForStream(cancellationToken);
    }

    internal async Task<HttpResponse> Send()
    {
        var request = _data.MapToHttpRequestMessage();

        var outputRequestResponse = false;
        HttpResponseMessage? response = null;
        string? responseBody = null;
        CookieContainer? responseCookies = null;

        try
        {
            var client = _httpClient;
            client.BaseAddress = _data.BaseUri;

            var cts = new CancellationTokenSource(_data.Timeout);


            if (request.Content != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync(cts.Token);
            }

            response = await client.SendAsync(request, cts.Token);
            responseBody = await response.Content.ReadAsStringAsync(cts.Token);

            responseCookies = ExtractResponseCookies(response, _data.Uri);

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

        return new HttpResponse(request, response, responseCookies, body: responseBody, _data.SerializerProvider);
    }

    internal async Task<HttpStreamResponse> SendForStream(CancellationToken cancellationToken = default)
    {
        var request = _data.MapToHttpRequestMessage();
        HttpResponseMessage? response = null;

        try
        {
            using var timeoutCts = new CancellationTokenSource(_data.Timeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

            // Use ResponseHeadersRead for streaming to avoid buffering the entire response
            response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token);

            return new HttpStreamResponse(response);
        }
        catch
        {
            response?.Dispose();
            throw;
        }
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
