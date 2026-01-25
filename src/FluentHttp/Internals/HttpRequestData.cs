using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Fuzn.FluentHttp.Internals;

internal class HttpRequestData
{
    internal HttpClient HttpClient { get; set; }
    internal Uri AbsoluteUri { get; set; }
    internal Uri BaseUri { get; set; }
    internal string RequestUrl { get; set; }
    internal string? ContentType { get; set; }
    internal HttpMethod Method { get; set; }
    internal object? Body { get; set; }
    internal string AcceptType { get; set; } = "application/json";
    internal List<Cookie> Cookies { get; set; } = new();
    internal Dictionary<string, string> Headers { get; set; } = new();
    internal Dictionary<string, object> Options { get; set; } = new();
    internal string UserAgent { get; set; }
    internal TimeSpan Timeout { get; set; }
    internal JsonSerializerOptions SerializerOptions { get; set; }
    internal ISerializerProvider SerializerProvider { get; set; }
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

        // Add Accept header
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

        // Handle Content-Type and body based on content type string
        if (Body != null && !string.IsNullOrEmpty(ContentType))
        {
            if (ContentType == "multipart/form-data")
            {
                request.Content = BuildMultipartContent();
            }
            else if (ContentType == "application/x-www-form-urlencoded" && Body is Dictionary<string, string> dictBody)
            {
                request.Content = new FormUrlEncodedContent(dictBody);
            }
            else if (ContentType == "application/octet-stream")
            {
                if (Body is byte[] byteArray)
                {
                    request.Content = new ByteArrayContent(byteArray);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                }
                else if (Body is Stream stream)
                {
                    request.Content = new StreamContent(stream);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                }
            }
            else
            {
                // Handle JSON, XML, plain text, and custom content types
                if (Body is string rawContent)
                {
                    request.Content = new StringContent(rawContent, Encoding.UTF8, ContentType);
                }
                else if (ContentType == "text/plain")
                {
                    request.Content = new StringContent(Body.ToString() ?? string.Empty, Encoding.UTF8, "text/plain");
                }
                else
                {
                    // Default to JSON serialization for objects
                    var jsonContent = JsonSerializer.Serialize(Body, SerializerOptions);
                    request.Content = new StringContent(jsonContent, Encoding.UTF8, ContentType);
                }
            }
        }

        request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);

        return request;
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
