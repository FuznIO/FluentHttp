namespace Fuzn.FluentHttp;

public static class HttpClientExtensions
{
    /// <summary>
    /// Provides fluent HTTP request building extensions for <see cref="HttpClient"/>.
    /// </summary>
    extension (HttpClient httpClient)
    {
        /// <summary>
        /// Creates a new <see cref="HttpRequestBuilder"/> with the specified URL.
        /// </summary>
        /// <param name="url">The URL for the HTTP request.</param>
        /// <returns>A new <see cref="HttpRequestBuilder"/> instance configured with the specified URL.</returns>
        public HttpRequestBuilder Url(string url)
        {
            return new HttpRequestBuilder(httpClient, url);
        }
    }
}
