namespace Fuzn.FluentHttp;

public static class HttpClientExtensions
{
    extension (HttpClient httpClient)
    {
        public HttpRequestBuilder Url(string url)
        {
            return new HttpRequestBuilder(httpClient, url);
        }
    }
}
