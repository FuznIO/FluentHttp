using System.Net;
using System.Text;
using System.Text.Json;

namespace Fuzn.FluentHttp.Internals;

internal class HttpRequestData
{
    internal ContentTypes ContentType { get; set; } = ContentTypes.Json;
    internal Uri Uri { get; set; }
    internal HttpMethod Method { get; set; }
    internal Authentication Auth { get; set; }
    internal object? Body { get; set; }
    internal AcceptTypes AcceptTypes { get; set; } = AcceptTypes.Json;
    internal List<Cookie> Cookies { get; set; } = new();
    internal Dictionary<string, string> Headers { get; set; } = new();
    internal Dictionary<string, object> Options { get; set; } = new();
    internal Action<HttpRequestMessage>? BeforeSend { get; set; }
    internal string UserAgent { get; set; }
    internal TimeSpan Timeout { get; set; }
    internal HttpClient? HttpClient { get; set; }
    internal JsonSerializerOptions SerializerOptions { get; set; }
    internal ISerializerProvider SerializerProvider { get; set; }
    internal Uri BaseUri
    {
        get
        {
            if (field == null)
                field = new UriBuilder(Uri.Scheme, Uri.Host, Uri.IsDefaultPort ? -1 : Uri.Port).Uri;

            return field;
        }
    }
    internal string RelativeUri
    {
        get
        {
            return Uri.PathAndQuery;
        }
    }

    public HttpRequestMessage MapToHttpRequestMessage()
    {
        var request = new HttpRequestMessage(Method, RelativeUri);
        
        foreach (var option in Options)
        {
            request.Options.Set(new HttpRequestOptionsKey<object>(option.Key), option.Value);
        }

        if (AcceptTypes == AcceptTypes.Json)
            request.Headers.Add("Accept", "application/json");
        else if (AcceptTypes == AcceptTypes.Html)
            request.Headers.Add("Accept", $"text/html,application/xhtml+xml");
        
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

            var cookieHeader = cookieContainer.GetCookieHeader(Uri);
            request.Headers.Remove("Cookie");
            if (!string.IsNullOrEmpty(cookieHeader))
                request.Headers.Add("Cookie", cookieHeader);
        }

        if (ContentType == ContentTypes.Json && Body != null)
        {
            if (Body is string rawJson)
            {
                request.Content = new StringContent(rawJson, Encoding.UTF8, "application/json");
            }
            else
            {
                var jsonContent = JsonSerializer.Serialize(Body, SerializerOptions);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }
        }
        else if (ContentType == ContentTypes.XFormUrlEncoded && Body is Dictionary<string, string> dictBody)
        {
            request.Content = new FormUrlEncodedContent(dictBody);
        }

        if (!string.IsNullOrEmpty(Auth?.BearerToken))
        {
            request.Headers.Remove("Authorization");
            request.Headers.Add("Authorization", $"Bearer {Auth.BearerToken}");
        }
        else if (!string.IsNullOrEmpty(Auth?.Basic))
        {
            request.Headers.Remove("Authorization");
            request.Headers.Add("Authorization", $"Basic {Auth.Basic}");
        }

        request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);

        return request;
    }
}
