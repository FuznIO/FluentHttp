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
public class HttpRequestData
{
    internal HttpClient HttpClient { get; set; } = null!;

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
    public List<Cookie> Cookies { get; internal set; } = [];

    /// <summary>Request headers.</summary>
    public Dictionary<string, string> Headers { get; internal set; } = new();

    /// <summary>Custom options/metadata for the request.</summary>
    public Dictionary<string, object> Options { get; internal set; } = new();

    /// <summary>The User-Agent header value.</summary>
    public string? UserAgent { get; internal set; }

    /// <summary>Request timeout.</summary>
    public TimeSpan Timeout { get; internal set; }

    /// <summary>The HTTP protocol version (e.g., HTTP/1.1, HTTP/2, HTTP/3).</summary>
    public Version? Version { get; internal set; }

    /// <summary>The HTTP version policy that controls upgrade/downgrade behavior.</summary>
    public HttpVersionPolicy? VersionPolicy { get; internal set; }

    /// <summary>JSON serializer options.</summary>
    public JsonSerializerOptions? SerializerOptions { get; internal set; }

    /// <summary>Custom serializer provider.</summary>
    public ISerializerProvider? SerializerProvider { get; internal set; }

    /// <summary>Files to upload.</summary>
    public List<FileContent> Files { get; internal set; } = [];

    /// <summary>Form fields for multipart requests.</summary>
    public Dictionary<string, string> FormFields { get; internal set; } = new();

    /// <summary>Query parameters.</summary>
    public List<KeyValuePair<string, string>> QueryParams { get; internal set; } = [];

    internal CancellationToken CancellationToken { get; set; } = default;

    /// <summary>Indicates whether the BeforeSend interceptor has been executed.</summary>
    internal bool InterceptorExecuted { get; set; }

    private string BuildQueryString()
    {
        var queryPairs = new List<string>();

        foreach (var param in QueryParams)
        {
            var encodedKey = HttpUtility.UrlEncode(param.Key);
            var encodedValue = HttpUtility.UrlEncode(param.Value);
            queryPairs.Add($"{encodedKey}={encodedValue}");
        }

        return string.Join("&", queryPairs);
    }

    internal HttpRequestMessage MapToHttpRequestMessage()
    {
        var request = new HttpRequestMessage(Method, GetRequestUrlWithPathAndQuery());

        if (Version is not null)
            request.Version = Version;

        if (VersionPolicy is not null)
            request.VersionPolicy = VersionPolicy.Value;

        foreach (var option in Options)
        {
            request.Options.Set(new HttpRequestOptionsKey<object>(option.Key), option.Value);
        }

        request.Headers.Add("Accept", AcceptType);

        foreach (var header in Headers)
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (Cookies is { Count: > 0 })
        {
            var cookieContainer = new CookieContainer();
            foreach (var cookie in Cookies)
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

        request.Content = MapContent();
        request.Content?.Headers.ContentType ??= new MediaTypeHeaderValue(ContentType);

        return request;
    }

    private HttpContent? MapContent()
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
        var jsonContent = SerializerProvider is not null
            ? SerializerProvider.Serialize(Content)
            : JsonSerializer.Serialize(Content, SerializerOptions);
        return new StringContent(jsonContent, Encoding.UTF8, ContentType);
    }

    private string GetRequestUrlWithPathAndQuery()
    {
        if (QueryParams.Count == 0)
            return AbsoluteUri.PathAndQuery;

        var pathAndQuery = AbsoluteUri.PathAndQuery;

        // Build query string
        var queryString = BuildQueryString();

        // Check if URL already has query parameters
        if (pathAndQuery.Contains('?'))
        {
            // Append with &
            return $"{pathAndQuery}&{queryString}";
        }
        else
        {
            // Remove existing query from pathAndQuery if present, add new one
            var path = pathAndQuery.Split('?')[0];
            return $"{path}?{queryString}";
        }
    }

    private MultipartFormDataContent BuildMultipartContent()
    {
        var multipartContent = new MultipartFormDataContent();

        // Add form fields
        foreach (var field in FormFields)
        {
            multipartContent.Add(new StringContent(field.Value, Encoding.UTF8), field.Key);
        }

        // Add files
        foreach (var file in Files)
        {
            var streamContent = new StreamContent(file.Content);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            multipartContent.Add(streamContent, file.Name, file.FileName);
        }

        return multipartContent;
    }
}
