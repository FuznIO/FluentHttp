using System.Web;

namespace Fuzn.FluentHttp.Testing;

/// <summary>
/// An immutable record of a request that was sent through a <see cref="FluentHttpMockHandler"/>.
/// Use it to assert on what FluentHttp actually sent (method, URL, headers, body).
/// </summary>
public sealed class CapturedRequest
{
    private readonly ISerializerProvider _serializer;

    internal CapturedRequest(
        HttpMethod method,
        Uri requestUri,
        IReadOnlyDictionary<string, string[]> headers,
        string? content,
        string? contentType,
        ISerializerProvider serializer)
    {
        Method = method;
        RequestUri = requestUri;
        Headers = headers;
        Content = content;
        ContentType = contentType;
        _serializer = serializer;
    }

    /// <summary>
    /// Gets the HTTP method of the request.
    /// </summary>
    public HttpMethod Method { get; }

    /// <summary>
    /// Gets the absolute request URI.
    /// </summary>
    public Uri RequestUri { get; }

    /// <summary>
    /// Gets all request headers, including content headers, keyed by header name.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Headers { get; }

    /// <summary>
    /// Gets the request body as a string, or <c>null</c> if the request had no body.
    /// </summary>
    public string? Content { get; }

    /// <summary>
    /// Gets the Content-Type of the request body, or <c>null</c> if there was no body.
    /// </summary>
    public string? ContentType { get; }

    /// <summary>
    /// Determines whether a header with the given name was present, optionally matching a value.
    /// </summary>
    /// <param name="name">The header name (case-insensitive).</param>
    /// <param name="value">The expected value, or <c>null</c> to match any value.</param>
    /// <returns><c>true</c> if a matching header was present; otherwise, <c>false</c>.</returns>
    public bool HasHeader(string name, string? value = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var match = Headers.FirstOrDefault(h => string.Equals(h.Key, name, StringComparison.OrdinalIgnoreCase));
        if (match.Key is null)
            return false;

        return value is null || match.Value.Contains(value, StringComparer.Ordinal);
    }

    /// <summary>
    /// Determines whether the request URL contained the given query parameter, optionally matching a value.
    /// </summary>
    /// <param name="name">The query parameter name.</param>
    /// <param name="value">The expected value, or <c>null</c> to match any value.</param>
    /// <returns><c>true</c> if a matching query parameter was present; otherwise, <c>false</c>.</returns>
    public bool HasQueryParam(string name, string? value = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var query = HttpUtility.ParseQueryString(RequestUri.Query);
        if (!query.AllKeys.Contains(name))
            return false;

        return value is null || string.Equals(query[name], value, StringComparison.Ordinal);
    }

    /// <summary>
    /// Deserializes the captured request body into the specified type using the handler's serializer.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the body into.</typeparam>
    /// <returns>The deserialized object, or default if the body is empty.</returns>
    public T? ContentAs<T>()
    {
        if (string.IsNullOrEmpty(Content))
            return default;

        return _serializer.Deserialize<T>(Content);
    }
}
