namespace Fuzn.FluentHttp;

/// <summary>
/// Provides extension methods for <see cref="HttpClient"/>.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Creates a new <see cref="FluentHttpRequest"/> for the specified HttpClient.
    /// </summary>
    /// <param name="httpClient">The HttpClient to use for the request.</param>
    /// <returns>A new <see cref="FluentHttpRequest"/> instance.</returns>
    public static FluentHttpRequest Request(this HttpClient httpClient)
    {
        return new FluentHttpRequest(httpClient);
    }

    /// <summary>
    /// Creates a new <see cref="FluentHttpRequest"/> with the specified URL.
    /// </summary>
    /// <param name="httpClient">The HttpClient to use for the request.</param>
    /// <param name="url">The URL for the HTTP request.</param>
    /// <returns>A new <see cref="FluentHttpRequest"/> instance configured with the specified URL.</returns>
    public static FluentHttpRequest Url(this HttpClient httpClient, string url)
    {
        return new FluentHttpRequest(httpClient).WithUrl(url);
    }
}
