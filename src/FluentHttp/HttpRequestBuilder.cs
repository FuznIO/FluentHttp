using Fuzn.FluentHttp.Internals;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Fuzn.FluentHttp;

/// <summary>
/// Fluent builder for constructing and sending HTTP requests.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public class HttpRequestBuilder
{
    private readonly HttpRequestData _data = new();

    /// <summary>
    /// Gets the underlying request data for inspection or direct modification.
    /// </summary>
    public HttpRequestData Data => _data;

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
    public HttpRequestBuilder WithSerializer(ISerializerProvider serializerProvider)
    {
        _data.SerializerProvider = serializerProvider;
        return this;
    }

    /// <summary>
    /// Sets custom JSON serializer options for the default System.Text.Json serializer.
    /// These options are used for request body serialization and response deserialization.
    /// Note: This is ignored if a custom <see cref="ISerializerProvider"/> is set via <see cref="WithSerializer"/>.
    /// </summary>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithJsonOptions(JsonSerializerOptions options)
    {
        _data.SerializerOptions = options;
        return this;
    }

    /// <summary>
    /// Sets the Content-Type header for the request.
    /// </summary>
    /// <param name="contentType">The content type to use.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithContentType(ContentTypes contentType)
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
    public HttpRequestBuilder WithContentType(string contentType)
    {
        _data.ContentType = contentType;
        return this;
    }

    /// <summary>
    /// Sets the request content. The content will be serialized based on the configured content type.
    /// </summary>
    /// <param name="content">The content to send.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithContent(object content)
    {
        _data.Content = content;
        
        // Auto-set Content-Type to JSON if not already set
        if (string.IsNullOrEmpty(_data.ContentType))
        {
            _data.ContentType = "application/json";
        }
        
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
    public HttpRequestBuilder WithFile(string name, string fileName, Stream content, string contentType = "application/octet-stream")
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
    public HttpRequestBuilder WithFile(string name, string fileName, byte[] content, string contentType = "application/octet-stream")
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
    public HttpRequestBuilder WithFile(FileContent file)
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
    public HttpRequestBuilder WithFormField(string name, string value)
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
    public HttpRequestBuilder WithFormFields(IDictionary<string, string> fields)
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
    public HttpRequestBuilder WithQueryParam(string key, object? value)
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
    public HttpRequestBuilder WithQueryParams(IDictionary<string, object?> parameters)
    {
        foreach (var param in parameters)
        {
            WithQueryParam(param.Key, param.Value);
        }

        return this;
    }

    /// <summary>
    /// Adds multiple values for the same query parameter (e.g., ?tags=c%23&amp;tags=dotnet&amp;tags=http).
    /// </summary>
    /// <param name="key">The parameter name.</param>
    /// <param name="values">The collection of values.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithQueryParam(string key, IEnumerable<object?>? values)
    {
        if (values == null)
            return this;

        foreach (var value in values)
        {
            WithQueryParam(key, value);
        }

        return this;
    }

    /// <summary>
    /// Adds query parameters from an anonymous object.
    /// </summary>
    /// <param name="parameters">An anonymous object whose properties become query parameters.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithQueryParams(object? parameters)
    {
        if (parameters == null)
            return this;

        var properties = parameters.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(parameters);
            WithQueryParam(prop.Name, value);
        }

        return this;
    }

    /// <summary>
    /// Sets the Accept header for the request.
    /// </summary>
    /// <param name="acceptTypes">The accepted response content types.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithAccept(AcceptTypes acceptTypes)
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
    public HttpRequestBuilder WithAccept(string acceptType)
    {
        _data.AcceptType = acceptType;
        return this;
    }

    /// <summary>
    /// Adds a cookie to the request.
    /// </summary>
    /// <param name="cookie">The cookie to add.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithCookie(Cookie cookie)
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
    public HttpRequestBuilder WithCookie(string name, string value, string? path = null, string? domain = null, TimeSpan? duration = null)
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
    /// </param>
    /// <param name="key">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithHeader(string key, string value)
    {
        _data.Headers[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple headers to the request.
    /// </summary>
    /// <param name="headers">A dictionary of header names and values.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithHeaders(IDictionary<string, string> headers)
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
    public HttpRequestBuilder WithAuthBearer(string token)
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
    public HttpRequestBuilder WithAuthBasic(string username, string password)
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
    public HttpRequestBuilder WithAuthApiKey(string apiKey, string headerName = "X-API-Key")
    {
        _data.Headers[headerName] = apiKey;
        return this;
    }

    /// <summary>
    /// Sets a custom User-Agent header for the request.
    /// </summary>
    /// <param name="userAgent">The User-Agent string.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithUserAgent(string userAgent)
    {
        _data.UserAgent = userAgent;
        return this;
    }

    /// <summary>
    /// Sets the timeout for the request.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithTimeout(TimeSpan timeout)
    {
        _data.Timeout = timeout;
        return this;
    }

    /// <summary>
    /// Sets the HTTP protocol version for the request.
    /// </summary>
    /// <param name="version">The HTTP version to use (e.g., HttpVersion.Version20 for HTTP/2).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithVersion(Version version)
    {
        _data.Version = version;
        return this;
    }

    /// <summary>
    /// Sets the HTTP protocol version policy for the request.
    /// Controls how the version is negotiated with the server.
    /// </summary>
    /// <param name="versionPolicy">The version policy that controls upgrade/downgrade behavior.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithVersionPolicy(HttpVersionPolicy versionPolicy)
    {
        _data.VersionPolicy = versionPolicy;
        return this;
    }

    /// <summary>
    /// Sets a cancellation token for the request. This token will be linked with any token passed to the HTTP method.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public HttpRequestBuilder WithCancellationToken(CancellationToken cancellationToken)
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
    public HttpRequestBuilder WithOption(string key, object value)
    {
        _data.Options.Add(key, value);
        return this;
    }

    /// <summary>
    /// Returns a formatted debug string showing the current request configuration.
    /// Useful for logging and debugging.
    /// Note: This shows the state BEFORE the BeforeSend interceptor runs.
    /// </summary>
    /// <returns>A formatted string containing the request configuration details.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== FluentHttp Request ===");
        sb.AppendLine($"Method: {_data.Method?.Method ?? "(not set)"}");
        sb.AppendLine($"URL: {_data.AbsoluteUri}");

        if (_data.QueryParams.Count > 0)
        {
            sb.AppendLine("Query Params:");
            foreach (var qp in _data.QueryParams)
                sb.AppendLine($"  {qp.Key} = {qp.Value}");
        }

        if (_data.Headers.Count > 0)
        {
            sb.AppendLine("Headers:");
            foreach (var h in _data.Headers)
                sb.AppendLine($"  {h.Key}: {(h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ? "[REDACTED]" : h.Value)}");
        }

        sb.AppendLine($"Content-Type: {_data.ContentType ?? "(not set)"}");
        sb.AppendLine($"Accept: {_data.AcceptType}");

        if (_data.Content != null)
        {
            var contentJson = _data.SerializerProvider?.Serialize(_data.Content)
                ?? JsonSerializer.Serialize(_data.Content, _data.SerializerOptions);
            sb.AppendLine($"Content: {(contentJson.Length > 500 ? contentJson[..500] + "..." : contentJson)}");
        }

        if (_data.Files.Count > 0)
        {
            sb.AppendLine($"Files: {_data.Files.Count}");
            foreach (var file in _data.Files)
                sb.AppendLine($"  {file.Name}: {file.FileName} ({file.ContentType})");
        }

        if (_data.FormFields.Count > 0)
        {
            sb.AppendLine("Form Fields:");
            foreach (var formField in _data.FormFields)
                sb.AppendLine($"  {formField.Key} = {formField.Value}");
        }

        if (_data.Cookies.Count > 0)
        {
            sb.AppendLine($"Cookies: {_data.Cookies.Count}");
            foreach (var cookie in _data.Cookies)
                sb.AppendLine($"  {cookie.Name} = {cookie.Value}");
        }

        if (_data.Timeout != TimeSpan.Zero)
            sb.AppendLine($"Timeout: {_data.Timeout}");

        if (!string.IsNullOrEmpty(_data.UserAgent))
            sb.AppendLine($"User-Agent: {_data.UserAgent}");

        if (_data.Version is not null)
            sb.AppendLine($"Version: HTTP/{_data.Version}");

        if (_data.VersionPolicy is not null)
            sb.AppendLine($"VersionPolicy: {_data.VersionPolicy}");

        return sb.ToString();
    }

    /// <summary>
    /// Builds the HttpRequestMessage without sending it.
    /// Useful for debugging, logging, or testing request construction.
    /// Runs the BeforeSend interceptor to show the exact request that would be sent.
    /// Note: Use this OR the Send methods (Get, Post, etc.), not both.
    /// For quick inspection without side effects, use <see cref="ToString"/> instead.
    /// </summary>
    /// <param name="method">The HTTP method to use.</param>
    /// <returns>The constructed HttpRequestMessage.</returns>
    public HttpRequestMessage BuildRequest(HttpMethod method)
    {
        _data.Method = method;
        FluentHttpDefaults.ExecuteInterceptor(this);
        return _data.MapToHttpRequestMessage();
    }

    /// <summary>
    /// Sends the request using the specified HTTP method.
    /// Useful for custom or non-standard HTTP methods (e.g., PROPFIND, MKCOL for WebDAV).
    /// </summary>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Send(HttpMethod method, CancellationToken cancellationToken = default)
    {
        _data.Method = method;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the specified HTTP method and deserializes the response to the specified type.
    /// Useful for custom or non-standard HTTP methods (e.g., PROPFIND, MKCOL for WebDAV).
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the typed HTTP response.</returns>
    public async Task<HttpResponse<T>> Send<T>(HttpMethod method, CancellationToken cancellationToken = default)
    {
        var response = await Send(method, cancellationToken);
        return new HttpResponse<T>(response);
    }

    /// <summary>
    /// Sends the request using the HTTP GET method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Get(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Get;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP GET method and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <returns>A task representing the asynchronous operation, containing the typed HTTP response.</returns>
    public async Task<HttpResponse<T>> Get<T>(CancellationToken cancellationToken = default)
    {
        var response = await Get(cancellationToken);
        return new HttpResponse<T>(response);
    }

    /// <summary>
    /// Sends the request using the HTTP POST method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Post(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Post;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP POST method and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <returns>A task representing the asynchronous operation, containing the typed HTTP response.</returns>
    public async Task<HttpResponse<T>> Post<T>(CancellationToken cancellationToken = default)
    {
        var response = await Post(cancellationToken);
        return new HttpResponse<T>(response);
    }

    /// <summary>
    /// Sends the request using the HTTP PUT method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Put(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Put;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP PUT method and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <returns>A task representing the asynchronous operation, containing the typed HTTP response.</returns>
    public async Task<HttpResponse<T>> Put<T>(CancellationToken cancellationToken = default)
    {
        var response = await Put(cancellationToken);
        return new HttpResponse<T>(response);
    }

    /// <summary>
    /// Sends the request using the HTTP DELETE method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Delete(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Delete;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP DELETE method and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <returns>A task representing the asynchronous operation, containing the typed HTTP response.</returns>
    public async Task<HttpResponse<T>> Delete<T>(CancellationToken cancellationToken = default)
    {
        var response = await Delete(cancellationToken);
        return new HttpResponse<T>(response);
    }

    /// <summary>
    /// Sends the request using the HTTP PATCH method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Patch(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Patch;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP PATCH method and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <returns>A task representing the asynchronous operation, containing the typed HTTP response.</returns>
    public async Task<HttpResponse<T>> Patch<T>(CancellationToken cancellationToken = default)
    {
        var response = await Patch(cancellationToken);
        return new HttpResponse<T>(response);
    }

    /// <summary>
    /// Sends the request using the HTTP HEAD method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Head(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Head;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP OPTIONS method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Options(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Options;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP TRACE method.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    public Task<HttpResponse> Trace(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Trace;
        return SendInternal(cancellationToken);
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

    /// <summary>
    /// Sends the request using the specified HTTP method and returns the response as a stream.
    /// Useful for custom or non-standard HTTP methods that return streaming content.
    /// The returned HttpStreamResponse should be disposed after use.
    /// </summary>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the streaming response.</returns>
    public Task<HttpStreamResponse> SendStream(HttpMethod method, CancellationToken cancellationToken = default)
    {
        _data.Method = method;
        return SendForStream(cancellationToken);
    }

    private async Task<HttpResponse> SendInternal(CancellationToken cancellationToken = default)
    {
        // Execute global interceptor before building request
        FluentHttpDefaults.ExecuteInterceptor(this);

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
        // Execute global interceptor before building request
        FluentHttpDefaults.ExecuteInterceptor(this);

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
