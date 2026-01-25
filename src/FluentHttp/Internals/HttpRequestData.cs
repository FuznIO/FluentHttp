using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Fuzn.FluentHttp.Internals;

internal class HttpRequestData
{
    internal HttpClient HttpClient { get; set; } = null!;
    internal Uri AbsoluteUri { get; set; } = null!;
    internal Uri BaseUri { get; set; } = null!;
    internal string RequestUrl { get; set; } = null!;
    internal string? ContentType { get; set; } = null;
    internal HttpMethod Method { get; set; } = null!;
    internal object? Body { get; set; } = null;
    internal string AcceptType { get; set; } = "application/json";
    internal List<Cookie> Cookies { get; set; } = new();
    internal Dictionary<string, string> Headers { get; set; } = new();
    internal Dictionary<string, object> Options { get; set; } = new();
    internal string? UserAgent { get; set; } = null;
    internal TimeSpan Timeout { get; set; }
    internal JsonSerializerOptions? SerializerOptions { get; set; }
    internal ISerializerProvider? SerializerProvider { get; set; }
    internal List<FileContent> Files { get; set; } = new();
    internal Dictionary<string, string> FormFields { get; set; } = new();
    internal List<KeyValuePair<string, string>> QueryParams { get; set; } = new();

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

    public HttpRequestMessage MapToHttpRequestMessage()
    {
        var request = new HttpRequestMessage(Method, GetRequestUrlWithPathAndQuery());
        
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
        if (request.Content?.Headers?.ContentType == null)
            request.Content?.Headers?.ContentType = new MediaTypeHeaderValue(ContentType);

        return request;
    }

    private HttpContent? MapContent()
    {
        // Multipart is handled separately and doesn't require Body
        if (ContentType == "multipart/form-data")
            return BuildMultipartContent();

        // For all other content types, Body is required
        if (Body is null)
            return null;

        // Handle form URL encoded
        if (ContentType == "application/x-www-form-urlencoded")
        {
            if (Body is Dictionary<string, string> dictBody)
                return new FormUrlEncodedContent(dictBody);
            
            if (Body is IEnumerable<KeyValuePair<string, string>> kvpBody)
                return new FormUrlEncodedContent(kvpBody);

            throw new InvalidOperationException(
                $"Body must be Dictionary<string, string> or IEnumerable<KeyValuePair<string, string>> for content type '{ContentType}'.");
        }

        // Handle binary content
        if (ContentType == "application/octet-stream")
        {
            return Body switch
            {
                byte[] byteArray => new ByteArrayContent(byteArray),
                Stream stream => new StreamContent(stream),
                _ => throw new InvalidOperationException(
                    $"Body must be byte[] or Stream for content type '{ContentType}'.")
            };
        }

        // Handle string body - use as-is for any content type
        if (Body is string stringContent)
            return new StringContent(stringContent, Encoding.UTF8, ContentType);

        // Handle plain text - convert object to string
        if (ContentType == "text/plain")
            return new StringContent(Body.ToString() ?? string.Empty, Encoding.UTF8, ContentType);

        // For JSON and other content types, serialize as JSON
        var jsonContent = JsonSerializer.Serialize(Body, SerializerOptions);
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
