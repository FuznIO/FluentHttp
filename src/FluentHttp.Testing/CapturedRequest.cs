using System.Web;

namespace Fuzn.FluentHttp.Testing;

/// <summary>
/// An immutable record of a request that was sent through a <see cref="MockHttpHandler"/>.
/// Use it to assert on what FluentHttp actually sent (method, URL, headers, query, body).
/// </summary>
public sealed class CapturedRequest
{
    private readonly Func<string?, ISerializerProvider> _resolveSerializer;

    internal CapturedRequest(
        HttpMethod method,
        Uri requestUri,
        IReadOnlyDictionary<string, string[]> headers,
        string? content,
        byte[]? contentBytes,
        string? contentType,
        Func<string?, ISerializerProvider> resolveSerializer)
    {
        Method = method;
        RequestUri = requestUri;
        Headers = headers;
        Query = ParseQuery(requestUri);
        Content = content;
        ContentBytes = contentBytes;
        ContentType = contentType;
        _resolveSerializer = resolveSerializer;
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
    /// Gets all request headers, including content headers, keyed by name (case-insensitive).
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Headers { get; }

    /// <summary>
    /// Gets the request's query parameters, parsed from the URL and keyed by name. A repeated parameter
    /// (e.g. <c>?tag=a&amp;tag=b</c>) keeps all of its values; use <see cref="RequestUri"/> for the raw string.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Query { get; }

    /// <summary>
    /// Gets the request body as a string, or <c>null</c> if the request had no body.
    /// Decoded using the request's declared charset, falling back to UTF-8. For binary
    /// payloads, prefer <see cref="ContentBytes"/>.
    /// </summary>
    public string? Content { get; }

    /// <summary>
    /// Gets the raw request body bytes, or <c>null</c> if the request had no body.
    /// Use this to assert on binary or multipart payloads that do not round-trip as text.
    /// </summary>
    public byte[]? ContentBytes { get; }

    /// <summary>
    /// Gets the Content-Type of the request body, or <c>null</c> if there was no body.
    /// </summary>
    public string? ContentType { get; }

    /// <summary>
    /// Deserializes the captured request body into the specified type using the serializer FluentHttp
    /// resolves for the request's Content-Type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the body into.</typeparam>
    /// <returns>The deserialized object, or default if the body is empty.</returns>
    public T? ContentAs<T>()
    {
        if (string.IsNullOrEmpty(Content))
            return default;

        return _resolveSerializer(ContentType).Deserialize<T>(Content);
    }

    private static IReadOnlyDictionary<string, string[]> ParseQuery(Uri requestUri)
    {
        var parsed = HttpUtility.ParseQueryString(requestUri.Query);
        var query = new Dictionary<string, string[]>(StringComparer.Ordinal);

        foreach (var key in parsed.AllKeys)
        {
            if (key is not null)
                query[key] = parsed.GetValues(key) ?? [];
        }

        return query;
    }
}
