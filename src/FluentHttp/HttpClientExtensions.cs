namespace Fuzn.FluentHttp;

/// <summary>
/// Provides extension methods for <see cref="HttpClient"/>.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Provides fluent HTTP request building extensions for <see cref="HttpClient"/>.
    /// </summary>
    extension (HttpClient httpClient)
    {
        /// <summary>
        /// Creates a new <see cref="FluentHttpRequest"/> with the specified URL.
        /// </summary>
        /// <param name="url">The URL for the HTTP request.</param>
        /// <returns>A new <see cref="FluentHttpRequest"/> instance configured with the specified URL.</returns>
        public FluentHttpRequest Url(string url)
        {
            return new FluentHttpRequest(httpClient, url);
        }
    }
}
