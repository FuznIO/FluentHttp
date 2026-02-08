using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Fuzn.FluentHttp;

/// <summary>
/// Contains all data for building an HTTP request.
/// Exposed to interceptors for inspection and modification.
/// </summary>
public class FluentHttpRequestData
{
    private List<Cookie>? _cookies;
    private Dictionary<string, string>? _headers;
    private Dictionary<string, object>? _options;
    private List<FileContent>? _files;
    private Dictionary<string, string>? _formFields;
    private List<KeyValuePair<string, string>>? _queryParams;

    internal bool HasCookies => _cookies is { Count: > 0 };
    internal bool HasHeaders => _headers is { Count: > 0 };
    internal bool HasOptions => _options is { Count: > 0 };
    internal bool HasFiles => _files is { Count: > 0 };
    internal bool HasFormFields => _formFields is { Count: > 0 };
    internal bool HasQueryParams => _queryParams is { Count: > 0 };
    internal HttpClient HttpClient { get; set; } = null!;
    internal bool RequiresAbsoluteUri { get; set; }

    /// <summary>The absolute URI being called.</summary>
    public Uri AbsoluteUri { get; internal set; } = null!;

    /// <summary>The base URI (scheme + host + port).</summary>
    public Uri BaseUri { get; internal set; } = null!;

    /// <summary>The original request URL/path.</summary>
    public string RequestUrl { get; internal set; } = null!;

    /// <summary>The Content-Type header value.</summary>
    public string? ContentType { get; internal set; }

    /// <summary>The HTTP method.</summary>
    public HttpMethod Method { get; internal set; } = null!;

    /// <summary>The request content.</summary>
    public object? Content { get; internal set; }

    /// <summary>The Accept header value.</summary>
    public string AcceptType { get; internal set; } = "application/json";

    /// <summary>Cookies to send with the request.</summary>
    public List<Cookie> Cookies => _cookies ??= [];

    /// <summary>Request headers.</summary>
    public Dictionary<string, string> Headers => _headers ??= new();

    /// <summary>Custom options/metadata for the request.</summary>
    public Dictionary<string, object> Options => _options ??= new();

    /// <summary>The User-Agent header value.</summary>
    public string? UserAgent { get; internal set; }

    /// <summary>Request timeout.</summary>
    public TimeSpan Timeout { get; internal set; }

    /// <summary>The HTTP protocol version (e.g., HTTP/1.1, HTTP/2, HTTP/3).</summary>
    public Version? Version { get; internal set; }

    /// <summary>The HTTP version policy that controls upgrade/downgrade behavior.</summary>
    public HttpVersionPolicy? VersionPolicy { get; internal set; }

    /// <summary>JSON serializer options for System.Text.Json.</summary>
    public JsonSerializerOptions? JsonOptions { get; internal set; }

    /// <summary>Custom serializer.</summary>
    public ISerializerProvider? Serializer { get; internal set; }

    /// <summary>Files to upload.</summary>
    public List<FileContent> Files => _files ??= [];

    /// <summary>Form fields for multipart requests.</summary>
    public Dictionary<string, string> FormFields => _formFields ??= new();

    /// <summary>Query parameters.</summary>
    public List<KeyValuePair<string, string>> QueryParams => _queryParams ??= [];

    internal CancellationToken CancellationToken { get; set; } = default;

    private string BuildQueryString()
    {
        if (!HasQueryParams)
            return string.Empty;

        var sb = new StringBuilder();
        for (int i = 0; i < _queryParams!.Count; i++)
        {
            if (i > 0)
                sb.Append('&');
            sb.Append(HttpUtility.UrlEncode(_queryParams[i].Key));
            sb.Append('=');
            sb.Append(HttpUtility.UrlEncode(_queryParams[i].Value));
        }
        return sb.ToString();
    }

    internal HttpRequestMessage MapToHttpRequestMessage(ISerializerProvider serializerProvider)
    {
        var request = new HttpRequestMessage(Method, GetRequestUrlWithPathAndQuery());
        if (Version is not null)
            request.Version = Version;

        if (VersionPolicy is not null)
            request.VersionPolicy = VersionPolicy.Value;

        if (HasOptions)
        {
            foreach (var option in _options!)
            {
                request.Options.Set(new HttpRequestOptionsKey<object>(option.Key), option.Value);
            }
        }

        request.Headers.Add("Accept", AcceptType);

        if (HasHeaders)
        {
            foreach (var header in _headers!)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (HasCookies)
        {
            var cookieContainer = new CookieContainer();
            foreach (var cookie in _cookies!)
            {
                if (string.IsNullOrEmpty(cookie.Domain))
                    cookie.Domain = BaseUri.Host;
                cookieContainer.Add(BaseUri, cookie);
            }

            var cookieHeader = cookieContainer.GetCookieHeader(AbsoluteUri);
            request.Headers.Remove("Cookie");
            if (!string.IsNullOrEmpty(cookieHeader))
                request.Headers.Add("Cookie", cookieHeader);
        }

        request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);

        if (string.IsNullOrEmpty(ContentType))
            return request;

        request.Content = MapContent(serializerProvider);
        request.Content?.Headers.ContentType ??= new MediaTypeHeaderValue(ContentType);

        return request;
    }

    private HttpContent? MapContent(ISerializerProvider serializerProvider)
    {
        // Multipart is handled separately and doesn't require Content
        if (ContentType == "multipart/form-data")
            return BuildMultipartContent();

        // For all other content types, Content is required
        if (Content is null)
            return null;

        // Handle form URL encoded
        if (ContentType == "application/x-www-form-urlencoded")
        {
            if (Content is Dictionary<string, string> dictBody)
                return new FormUrlEncodedContent(dictBody);

            if (Content is IEnumerable<KeyValuePair<string, string>> kvpBody)
                return new FormUrlEncodedContent(kvpBody);

            throw new InvalidOperationException(
                $"Content must be Dictionary<string, string> or IEnumerable<KeyValuePair<string, string>> for content type '{ContentType}'.");
        }

        // Handle binary content
        if (ContentType == "application/octet-stream")
        {
            return Content switch
            {
                byte[] byteArray => new ByteArrayContent(byteArray),
                Stream stream => new StreamContent(stream),
                _ => throw new InvalidOperationException(
                    $"Content must be byte[] or Stream for content type '{ContentType}'.")
            };
        }

        // Handle string content - use as-is for any content type
        if (Content is string stringContent)
            return new StringContent(stringContent, Encoding.UTF8, ContentType);

        // Handle plain text - convert object to string
        if (ContentType == "text/plain")
            return new StringContent(Content.ToString() ?? string.Empty, Encoding.UTF8, ContentType);

        // For JSON and other content types, serialize as JSON
        // Use SerializerProvider if set, otherwise fall back to SerializerOptions
        string jsonContent = serializerProvider.Serialize(Content);
        return new StringContent(jsonContent, Encoding.UTF8, ContentType);
    }

    private string GetRequestUrlWithPathAndQuery()
    {
        if (!HasQueryParams)
            return RequiresAbsoluteUri ? AbsoluteUri.AbsoluteUri : AbsoluteUri.PathAndQuery;

        var pathAndQuery = AbsoluteUri.PathAndQuery;

        // Build query string
        var queryString = BuildQueryString();

        string resultPath;
        // Check if URL already has query parameters
        if (pathAndQuery.Contains('?'))
        {
            // Append with &
            resultPath = $"{pathAndQuery}&{queryString}";
        }
        else
        {
            // Remove existing query from pathAndQuery if present, add new one
            var path = pathAndQuery.Split('?')[0];
            resultPath = $"{path}?{queryString}";
        }

        if (RequiresAbsoluteUri)
        {
            // Combine base URI with the path and query
            return new Uri(BaseUri, resultPath).AbsoluteUri;
        }

        return resultPath;
    }

    private MultipartFormDataContent BuildMultipartContent()
    {
        var multipartContent = new MultipartFormDataContent();

        // Add form fields
        if (HasFormFields)
        {
            foreach (var field in _formFields!)
            {
                multipartContent.Add(new StringContent(field.Value, Encoding.UTF8), field.Key);
            }
        }

        // Add files
        if (HasFiles)
        {
            foreach (var file in _files!)
            {
                var streamContent = new StreamContent(file.Content);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                multipartContent.Add(streamContent, file.Name, file.FileName);
            }
        }

        return multipartContent;
    }
}
