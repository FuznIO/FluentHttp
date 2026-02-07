using Fuzn.FluentHttp.Internals;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Fuzn.FluentHttp;

/// <summary>
/// Fluent builder for constructing and sending HTTP requests.
/// </summary>
public class FluentHttpRequest
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

    private readonly HttpRequestData _data = new();

    /// <summary>
    /// Gets the underlying request data for inspection or direct modification.
    /// </summary>
    public HttpRequestData Data => _data;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentHttpRequest"/> class.
    /// </summary>
    /// <param name="httpClient">The HttpClient instance to use for sending requests.</param>
    /// <param name="url">The target URL for the request.</param>
    internal FluentHttpRequest(HttpClient httpClient, string url)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(url);

        _data.HttpClient = httpClient;
        _data.RequestUrl = url;

        if (httpClient.BaseAddress is null)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var absoluteUri))
                throw new ArgumentException("The provided URL is not a valid absolute URL and the HttpClient does not have a BaseAddress set.");

            _data.AbsoluteUri = absoluteUri;
            _data.BaseUri = new UriBuilder(absoluteUri.Scheme, absoluteUri.Host, absoluteUri.IsDefaultPort ? -1 : absoluteUri.Port).Uri;
            _data.RequiresAbsoluteUri = true;
        }
        else
        {
            _data.BaseUri = httpClient.BaseAddress;
            _data.AbsoluteUri = new Uri(httpClient.BaseAddress, url);
        }
    }

    /// <summary>
    /// Sets a custom serializer provider for request/response body serialization.
    /// </summary>
    /// <param name="serializer">The serializer provider to use.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FluentHttpRequest WithSerializer(ISerializerProvider serializer)
    {
        _data.Serializer = serializer;
        return this;
    }

    /// <summary>
    /// Sets custom JSON serializer options for the default System.Text.Json serializer.
    /// These options are used for request body serialization and response deserialization.
    /// Note: This is ignored if a custom <see cref="ISerializerProvider"/> is set via <see cref="WithSerializer"/>.
    /// </summary>
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FluentHttpRequest WithJsonOptions(JsonSerializerOptions options)
    {
        _data.JsonOptions = options;
        return this;
    }

    /// <summary>
    /// Sets the Content-Type header for the request.
    /// </summary>
    /// <param name="contentType">The content type to use.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FluentHttpRequest WithContentType(ContentTypes contentType)
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
    public FluentHttpRequest WithContentType(string contentType)
    {
        _data.ContentType = contentType;
        return this;
    }

    /// <summary>
    /// Sets the request content. The content will be serialized based on the configured content type.
    /// For JSON content types, the object will be serialized using the configured serializer.
    /// </summary>
    /// <param name="content">The content to send.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// Serialization occurs when the request is sent. If serialization fails, 
    /// a <see cref="FluentHttpSerializationException"/> will be thrown.
    /// </remarks>
    public FluentHttpRequest WithContent(object content)
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
    public FluentHttpRequest WithFile(string name, string fileName, Stream content, string contentType = "application/octet-stream")
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
    public FluentHttpRequest WithFile(string name, string fileName, byte[] content, string contentType = "application/octet-stream")
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
    public FluentHttpRequest WithFile(FileContent file)
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
    public FluentHttpRequest WithFormField(string name, string value)
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
    public FluentHttpRequest WithFormFields(IDictionary<string, string> fields)
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
    public FluentHttpRequest WithQueryParam(string key, object? value)
    {
        if (value != null)
        {
            var stringValue = value switch
            {
                string s => s,
                bool b => b ? "true" : "false",
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
    public FluentHttpRequest WithQueryParams(IDictionary<string, object?> parameters)
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
    public FluentHttpRequest WithQueryParam(string key, IEnumerable<object?>? values)
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
    public FluentHttpRequest WithQueryParams(object? parameters)
    {
        if (parameters == null)
            return this;

        var type = parameters.GetType();
        var properties = _propertyCache.GetOrAdd(type, t => t.GetProperties());

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
    public FluentHttpRequest WithAccept(AcceptTypes acceptTypes)
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
    public FluentHttpRequest WithAccept(string acceptType)
    {
        _data.AcceptType = acceptType;
        return this;
    }

    /// <summary>
    /// Adds a cookie to the request.
    /// </summary>
    /// <param name="cookie">The cookie to add.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FluentHttpRequest WithCookie(Cookie cookie)
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
    public FluentHttpRequest WithCookie(string name, string value, string? path = null, string? domain = null, TimeSpan? duration = null)
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
    public FluentHttpRequest WithHeader(string key, string value)
    {
        _data.Headers[key] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple headers to the request.
    /// </summary>
    /// <param name="headers">A dictionary of header names and values.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FluentHttpRequest WithHeaders(IDictionary<string, string> headers)
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
    public FluentHttpRequest WithAuthBearer(string token)
    {
        if (_data.HasHeaders && _data.Headers.ContainsKey("Authorization"))
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
    public FluentHttpRequest WithAuthBasic(string username, string password)
    {
        if (_data.HasHeaders && _data.Headers.ContainsKey("Authorization"))
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
    public FluentHttpRequest WithAuthApiKey(string apiKey, string headerName = "X-API-Key")
    {
        _data.Headers[headerName] = apiKey;
        return this;
    }

    /// <summary>
    /// Sets a custom User-Agent header for the request.
    /// </summary>
    /// <param name="userAgent">The User-Agent string.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FluentHttpRequest WithUserAgent(string userAgent)
    {
        _data.UserAgent = userAgent;
        return this;
    }

    /// <summary>
    /// Sets the timeout for the request.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FluentHttpRequest WithTimeout(TimeSpan timeout)
    {
        _data.Timeout = timeout;
        return this;
    }

    /// <summary>
    /// Sets the HTTP protocol version for the request.
    /// </summary>
    /// <param name="version">The HTTP version to use (e.g., HttpVersion.Version20 for HTTP/2).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FluentHttpRequest WithVersion(Version version)
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
    public FluentHttpRequest WithVersionPolicy(HttpVersionPolicy versionPolicy)
    {
        _data.VersionPolicy = versionPolicy;
        return this;
    }

    /// <summary>
    /// Sets a cancellation token for the request. This token will be linked with any token passed to the HTTP method.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FluentHttpRequest WithCancellationToken(CancellationToken cancellationToken)
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
    public FluentHttpRequest WithOption(string key, object value)
    {
        _data.Options.Add(key, value);
        return this;
    }

    /// <summary>
    /// Applies settings from a <see cref="FluentHttpSettings"/> instance.
    /// Settings are used as defaults and can be overridden by per-request configuration.
    /// </summary>
    /// <param name="settings">The settings to apply.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public FluentHttpRequest WithSettings(FluentHttpSettings settings)
    {
        _data.Settings = settings;
        return this;
    }

    /// <summary>
    /// Returns a formatted debug string showing the current request configuration.
    /// Useful for logging and debugging.
    /// </summary>
    /// <returns>A formatted string containing the request configuration details.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== FluentHttp Request ===");
        sb.AppendLine($"Method: {_data.Method?.Method ?? "(not set)"}");
        sb.AppendLine($"URL: {_data.AbsoluteUri}");

        if (_data.HasQueryParams)
        {
            sb.AppendLine("Query Params:");
            foreach (var qp in _data.QueryParams)
                sb.AppendLine($"  {qp.Key} = {qp.Value}");
        }

        if (_data.HasHeaders)
        {
            sb.AppendLine("Headers:");
            foreach (var h in _data.Headers)
                sb.AppendLine($"  {h.Key}: {(h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ? "[REDACTED]" : h.Value)}");
        }

        sb.AppendLine($"Content-Type: {_data.ContentType ?? "(not set)"}");
        sb.AppendLine($"Accept: {_data.AcceptType}");

        if (_data.Content != null)
        {
            var contentJson = _data.Serializer?.Serialize(_data.Content)
                ?? JsonSerializer.Serialize(_data.Content, _data.JsonOptions);
            sb.AppendLine($"Content: {(contentJson.Length > 500 ? contentJson[..500] + "..." : contentJson)}");
        }

        if (_data.HasFiles)
        {
            sb.AppendLine($"Files: {_data.Files.Count}");
            foreach (var file in _data.Files)
                sb.AppendLine($"  {file.Name}: {file.FileName} ({file.ContentType})");
        }

        if (_data.HasFormFields)
        {
            sb.AppendLine("Form Fields:");
            foreach (var formField in _data.FormFields)
                sb.AppendLine($"  {formField.Key} = {formField.Value}");
        }

        if (_data.HasCookies)
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
    /// Note: Use this OR the Send methods (Get, Post, etc.), not both.
    /// For quick inspection without side effects, use <see cref="ToString"/> instead.
    /// </summary>
    /// <param name="method">The HTTP method to use.</param>
    /// <returns>The constructed HttpRequestMessage.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization fails.</exception>
    public HttpRequestMessage BuildRequest(HttpMethod method)
    {
        _data.Method = method;
        return _data.MapToHttpRequestMessage(GetSerializer());
    }

    /// <summary>
    /// Sends the request using the specified HTTP method.
    /// Useful for custom or non-standard HTTP methods (e.g., PROPFIND, MKCOL for WebDAV).
    /// </summary>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization fails.</exception>
    public Task<FluentHttpResponse> Send(HttpMethod method, CancellationToken cancellationToken = default)
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
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization or response deserialization fails.</exception>
    public async Task<FluentHttpResponse<T>> Send<T>(HttpMethod method, CancellationToken cancellationToken = default)
    {
        var response = await Send(method, cancellationToken);
        return new FluentHttpResponse<T>(response);
    }

    /// <summary>
    /// Sends the request using the HTTP GET method.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization fails.</exception>
    public Task<FluentHttpResponse> Get(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Get;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP GET method and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the typed HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization or response deserialization fails.</exception>
    public async Task<FluentHttpResponse<T>> Get<T>(CancellationToken cancellationToken = default)
    {
        var response = await Get(cancellationToken);
        return new FluentHttpResponse<T>(response);
    }

    /// <summary>
    /// Sends the request using the HTTP POST method.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization fails.</exception>
    public Task<FluentHttpResponse> Post(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Post;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP POST method and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the typed HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization or response deserialization fails.</exception>
    public async Task<FluentHttpResponse<T>> Post<T>(CancellationToken cancellationToken = default)
    {
        var response = await Post(cancellationToken);
        return new FluentHttpResponse<T>(response);
    }

    /// <summary>
    /// Sends the request using the HTTP PUT method.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization fails.</exception>
    public Task<FluentHttpResponse> Put(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Put;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP PUT method and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the typed HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization or response deserialization fails.</exception>
    public async Task<FluentHttpResponse<T>> Put<T>(CancellationToken cancellationToken = default)
    {
        var response = await Put(cancellationToken);
        return new FluentHttpResponse<T>(response);
    }

    /// <summary>
    /// Sends the request using the HTTP DELETE method.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization fails.</exception>
    public Task<FluentHttpResponse> Delete(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Delete;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP DELETE method and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the typed HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization or response deserialization fails.</exception>
    public async Task<FluentHttpResponse<T>> Delete<T>(CancellationToken cancellationToken = default)
    {
        var response = await Delete(cancellationToken);
        return new FluentHttpResponse<T>(response);
    }

    /// <summary>
    /// Sends the request using the HTTP PATCH method.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization fails.</exception>
    public Task<FluentHttpResponse> Patch(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Patch;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP PATCH method and deserializes the response to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response body into.</typeparam>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the typed HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization or response deserialization fails.</exception>
    public async Task<FluentHttpResponse<T>> Patch<T>(CancellationToken cancellationToken = default)
    {
        var response = await Patch(cancellationToken);
        return new FluentHttpResponse<T>(response);
    }

    /// <summary>
    /// Sends the request using the HTTP HEAD method.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization fails.</exception>
    public Task<FluentHttpResponse> Head(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Head;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP OPTIONS method.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization fails.</exception>
    public Task<FluentHttpResponse> Options(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Options;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the HTTP TRACE method.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the HTTP response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization fails.</exception>
    public Task<FluentHttpResponse> Trace(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Trace;
        return SendInternal(cancellationToken);
    }

    /// <summary>
    /// Downloads the response as a stream using HTTP GET. 
    /// The returned FluentHttpStreamResponse should be disposed after use.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the streaming response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization fails.</exception>
    public Task<FluentHttpStreamResponse> GetStream(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Get;
        return SendForStream(cancellationToken);
    }

    /// <summary>
    /// Sends a POST request and returns the response as a stream.
    /// The returned FluentHttpStreamResponse should be disposed after use.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the streaming response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization fails.</exception>
    public Task<FluentHttpStreamResponse> PostStream(CancellationToken cancellationToken = default)
    {
        _data.Method = HttpMethod.Post;
        return SendForStream(cancellationToken);
    }

    /// <summary>
    /// Sends the request using the specified HTTP method and returns the response as a stream.
    /// Useful for custom or non-standard HTTP methods that return streaming content.
    /// The returned FluentHttpStreamResponse should be disposed after use.
    /// </summary>
    /// <param name="method">The HTTP method to use.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation, containing the streaming response.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when request content serialization fails.</exception>
    public Task<FluentHttpStreamResponse> SendStream(HttpMethod method, CancellationToken cancellationToken = default)
    {
        _data.Method = method;
        return SendForStream(cancellationToken);
    }

    private async Task<FluentHttpResponse> SendInternal(CancellationToken cancellationToken = default)
    {
        var serializerProvider = GetSerializer();

        var request = _data.MapToHttpRequestMessage(serializerProvider);

        var (linkedToken, linkedCts) = GetLinkedCancellationToken(cancellationToken);
        linkedToken.ThrowIfCancellationRequested();

        try
        {
            var response = await _data.HttpClient.SendAsync(request, linkedToken);
            var responseBytes = await response.Content.ReadAsByteArrayAsync(linkedToken);

            var responseCookies = ExtractResponseCookies(response, _data.AbsoluteUri);

            return new FluentHttpResponse(request, response, responseCookies, rawBytes: responseBytes, serializerProvider);
        }
        finally
        {
            linkedCts?.Dispose();
        }
    }

    private ISerializerProvider GetSerializer()
    {
        // Priority: per-request > instance settings > global settings > default
        if (_data.Serializer is not null)
            return _data.Serializer;

        if (_data.JsonOptions is not null)
            return new SystemTextJsonSerializerProvider(_data.JsonOptions);

        if (_data.Settings?.Serializer is not null)
            return _data.Settings.Serializer;

        if (_data.Settings?.JsonOptions is not null)
            return new SystemTextJsonSerializerProvider(_data.Settings.JsonOptions);

        if (FluentHttpDefaults.Settings.Serializer is not null)
            return FluentHttpDefaults.Settings.Serializer;

        if (FluentHttpDefaults.Settings.JsonOptions is not null)
            return new SystemTextJsonSerializerProvider(FluentHttpDefaults.Settings.JsonOptions);

        return new SystemTextJsonSerializerProvider();
    }

    private async Task<FluentHttpStreamResponse> SendForStream(CancellationToken cancellationToken = default)
    {
        var request = _data.MapToHttpRequestMessage(GetSerializer());
        HttpResponseMessage? response = null;

        var (linkedToken, linkedCts) = GetLinkedCancellationToken(cancellationToken);
        linkedToken.ThrowIfCancellationRequested();

        try
        {
            // Use ResponseHeadersRead for streaming to avoid buffering the entire response
            response = await _data.HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedToken);

            return new FluentHttpStreamResponse(response);
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
