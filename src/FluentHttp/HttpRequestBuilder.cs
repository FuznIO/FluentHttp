using Fuzn.FluentHttp.Internals;
using System.Net;
using System.Text;

namespace Fuzn.FluentHttp;

/// <summary>
/// Fluent builder for constructing and sending HTTP requests.
/// </summary>
public class HttpRequestBuilder
{
    private readonly HttpRequestData _data = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpRequestBuilder"/> class.
    /// </summary>
    /// <param name="httpClient">The HttpClient instance to use for sending requests.</param>
    /// <param name="url">The target URL for the request.</param>
    internal HttpRequestBuilder(HttpClient httpClient, string url)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(url);

        _data.HttpClient = httpClient;

        if (httpClient.BaseAddress == null)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new ArgumentException("The provided URL is not a valid absolute URL and the HttpClient does not have a BaseAddress set.");

            if (uri == null)
                throw new ArgumentException("The provided URL is not a valid absolute URL and the HttpClient does not have a BaseAddress set.");

            _data.BaseUri = new UriBuilder(uri.Scheme, uri.Host, uri.IsDefaultPort ? -1 : uri.Port).Uri;
            _data.RequestUrl = url;
            _data.AbsoluteUri = uri;
        }
        else
        {
            _data.BaseUri = httpClient.BaseAddress;
            _data.RequestUrl = url;
            _data.AbsoluteUri = new Uri(httpClient.BaseAddress, url);
        }
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
        _data.ContentType = contentType switch
        {
            ContentTypes.Json => "application/json",
            ContentTypes.Xml => "application/xml",
            ContentTypes.PlainText => "text/plain",
            ContentTypes.XFormUrlEncoded => "application/x-www-form-urlencoded",
            ContentTypes.Multipart => "multipart/form-data",
            ContentTypes.OctetStream => "application/octet-stream",
            _ => "application/json"
        };
        return this;
    }

    /// <summary>
    /// Sets a custom Content-Type header for the request.
    /// </summary>
    /// <param name="contentType">The custom content type string (e.g., "application/graphql", "application/x-yaml").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder ContentType(string contentType)
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
        
        // Auto-set Content-Type to JSON if not already set
        if (string.IsNullOrEmpty(_data.ContentType))
        {
            _data.ContentType = "application/json";
        }
        
        return this;
    }

    /// <summary>
    /// Configures the request as multipart/form-data for file uploads.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder AsMultipart()
    {
        _data.ContentType = "multipart/form-data";
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
        _data.ContentType = "multipart/form-data";
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
        _data.ContentType = "multipart/form-data";
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
        _data.ContentType = "multipart/form-data";
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
        _data.ContentType = "multipart/form-data";
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
        _data.ContentType = "multipart/form-data";
        foreach (var field in fields)
            _data.FormFields[field.Key] = field.Value;
        return this;
    }

    /// <summary>
    /// Adds a query parameter to the request URL.
    /// </summary>
    /// <param name="key">The parameter name.</param>
    /// <param name="value">The parameter value. Will be converted to string and URL-encoded.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder QueryParam(string key, object? value)
    {
        if (value != null)
        {
            var stringValue = value switch
            {
                string s => s,
                bool b => b.ToString().ToLowerInvariant(),
                DateTime dt => dt.ToString("O"), // ISO 8601 format
                DateTimeOffset dto => dto.ToString("O"),
                _ => value.ToString()
            };

            _data.QueryParams.Add(new KeyValuePair<string, string>(key, stringValue ?? string.Empty));
        }

        return this;
    }

    /// <summary>
    /// Adds multiple query parameters to the request URL.
    /// </summary>
    /// <param name="parameters">A dictionary of parameter names and values.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder QueryParams(IDictionary<string, object?> parameters)
    {
        foreach (var param in parameters)
        {
            QueryParam(param.Key, param.Value);
        }

        return this;
    }

    /// <summary>
    /// Adds multiple values for the same query parameter (e.g., ?tags=c%23&amp;tags=dotnet&amp;tags=http).
    /// </summary>
    /// <param name="key">The parameter name.</param>
    /// <param name="values">The collection of values.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder QueryParam(string key, IEnumerable<object?>? values)
    {
        if (values == null)
            return this;

        foreach (var value in values)
        {
            QueryParam(key, value);
        }

        return this;
    }

    /// <summary>
    /// Adds query parameters from an anonymous object.
    /// </summary>
    /// <param name="parameters">An anonymous object whose properties become query parameters.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder QueryParams(object? parameters)
    {
        if (parameters == null)
            return this;

        var properties = parameters.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(parameters);
            QueryParam(prop.Name, value);
        }

        return this;
    }

    /// <summary>
    /// Sets the Accept header for the request.
    /// </summary>
    /// <param name="acceptTypes">The accepted response content types.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Accept(AcceptTypes acceptTypes)
    {
        _data.AcceptType = acceptTypes switch
        {
            AcceptTypes.Json => "application/json",
            AcceptTypes.Html => "text/html,application/xhtml+xml",
            AcceptTypes.Xml => "application/xml",
            AcceptTypes.PlainText => "text/plain",
            AcceptTypes.Any => "*/*",
            AcceptTypes.OctetStream => "application/octet-stream",
            _ => "application/json"
        };
        return this;
    }

    /// <summary>
    /// Sets a custom Accept header for the request.
    /// </summary>
    /// <param name="acceptType">The custom accept type string (e.g., "application/pdf", "image/png").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Accept(string acceptType)
    {
        _data.AcceptType = acceptType;
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
    /// <param name="duration">The duration until the cookie expires. If not specified, creates a session cookie (no expiration).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Cookie(string name, string value, string? path = null, string? domain = null, TimeSpan? duration = null)
    {
        var cookie = new Cookie(name, value, path, domain);
        
        if (duration.HasValue)
        {
            cookie.Expires = DateTime.UtcNow.Add(duration.Value);
        }
        // If duration is not specified, don't set Expires - creates a session cookie
        
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
        if (_data.Headers.ContainsKey("Authorization"))
            throw new InvalidOperationException("Authentication is already configured.");

        _data.Headers["Authorization"] = $"Bearer {token}";
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
        if (_data.Headers.ContainsKey("Authorization"))
            throw new InvalidOperationException("Authentication is already configured.");

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        _data.Headers["Authorization"] = $"Basic {credentials}";
        return this;
    }

    /// <summary>
    /// Sets API key authentication via a custom header.
    /// </summary>
    /// <param name="apiKey">The API key value.</param>
    /// <param name="headerName">The header name. Defaults to "X-API-Key".</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder AuthApiKey(string apiKey, string headerName = "X-API-Key")
    {
        _data.Headers[headerName] = apiKey;
        return this;
    }

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

    /// <summary>
    /// Sets a cancellation token for the request. This token will be linked with any token passed to the HTTP method.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder CancellationToken(CancellationToken cancellationToken)
    {
        _data.CancellationToken = cancellationToken;
        return this;
    }

    /// <summary>
    /// Adds a custom option to the HTTP request. Options can be used to store additional metadata or configuration for the request.
    /// </summary>
    /// <param name="key">The option key.</param>
    /// <param name="value">The option value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder Options(string key, object value)
    {
        _data.Options.Add(key, value);
        return this;
    }

    /// <summary>
    /// Sends the request using the HTTP GET method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Get(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Get;
        return Send(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP POST method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Post(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Post;
        return Send(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP PUT method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Put(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Put;
        return Send(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP DELETE method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Delete(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Delete;
        return Send(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP PATCH method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Patch(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Patch;
        return Send(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP HEAD method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Head(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Head;
        return Send(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP OPTIONS method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Options(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Options;
        return Send(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP TRACE method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Trace(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Trace;
        return Send(cancellationToken);
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

    private async Task<HttpResponse> Send(CancellationToken cancellationToken = default)
    {
        var request = _data.MapToHttpRequestMessage();

        var (linkedToken, linkedCts) = GetLinkedCancellationToken(cancellationToken);
        linkedToken.ThrowIfCancellationRequested();

        try
        {
            var response = await _data.HttpClient.SendAsync(request, linkedToken);
            var responseBytes = await response.Content.ReadAsByteArrayAsync(linkedToken);

            var responseCookies = ExtractResponseCookies(response, _data.AbsoluteUri);

            var serializerProvider = _data.SerializerProvider ?? _data.SerializerOptions switch
            {
                null => new SystemTextJsonSerializerProvider(),
                var options => new SystemTextJsonSerializerProvider(options)
            };

            return new HttpResponse(request, response, responseCookies, rawBytes: responseBytes, serializerProvider);
        }
        finally
        {
            linkedCts?.Dispose();
        }
    }

    private async Task<HttpStreamResponse> SendForStream(CancellationToken cancellationToken = default)
    {
        var request = _data.MapToHttpRequestMessage();
        HttpResponseMessage? response = null;

        var (linkedToken, linkedCts) = GetLinkedCancellationToken(cancellationToken);
        linkedToken.ThrowIfCancellationRequested();

        try
        {
            // Use ResponseHeadersRead for streaming to avoid buffering the entire response
            response = await _data.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedToken);

            return new HttpStreamResponse(response);
        }
        catch
        {
            linkedCts?.Dispose();
            response?.Dispose();
            throw;
        }
    }

    private (CancellationToken Token, CancellationTokenSource? Cts) GetLinkedCancellationToken(CancellationToken cancellationToken)
    {
        var hasBuilderToken = _data.CancellationToken != default;
        var hasTimeout = _data.Timeout != TimeSpan.Zero;

        // No builder-level token or timeout - just use the method-level token directly
        if (!hasBuilderToken && !hasTimeout)
        {
            return (cancellationToken, null);
        }

        // Create linked token source combining all applicable tokens
        var linkedCts = hasBuilderToken
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _data.CancellationToken)
            : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        if (hasTimeout)
        {
            linkedCts.CancelAfter(_data.Timeout);
        }

        return (linkedCts.Token, linkedCts);
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
