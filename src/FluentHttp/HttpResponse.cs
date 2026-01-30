using System.Net;
using System.Net.Http.Headers;

namespace Fuzn.FluentHttp;

/// <summary>
/// Represents an HTTP response received from executing an HTTP request.
/// </summary>
public class HttpResponse
{
    private readonly List<Cookie> _cookies = [];
    private readonly HttpRequestMessage _request;
    private readonly ISerializerProvider _serializerProvider;
    private readonly byte[] _rawBytes;

    internal HttpResponse(HttpRequestMessage request,
        HttpResponseMessage response,
        CookieContainer? cookieContainer,
        byte[] rawBytes,
        ISerializerProvider serializerProvider)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(rawBytes);
        ArgumentNullException.ThrowIfNull(serializerProvider);

        _request = request;
        _rawBytes = rawBytes;
        _serializerProvider = serializerProvider;
        InnerResponse = response;
        RawResponse = InnerResponse.ToString();

        // Decode the content using the encoding specified in Content-Type header
        Content = DecodeContent(rawBytes, response.Content.Headers.ContentType);

        if (cookieContainer != null)
        {
            foreach (Cookie cookie in cookieContainer.GetAllCookies())
            {
                _cookies.Add(cookie);
            }
        }
    }

    /// <summary>
    /// Copy constructor for creating derived types.
    /// </summary>
    /// <param name="other">The HttpResponse to copy from.</param>
    protected HttpResponse(HttpResponse other)
    {
        ArgumentNullException.ThrowIfNull(other);

        _request = other._request;
        _rawBytes = other._rawBytes;
        _serializerProvider = other._serializerProvider;
        _cookies = other._cookies;
        InnerResponse = other.InnerResponse;
        RawResponse = other.RawResponse;
        Content = other.Content;
    }

    /// <summary>
    /// Gets the underlying <see cref="HttpResponseMessage"/>.
    /// </summary>
    public HttpResponseMessage InnerResponse { get; }

    /// <summary>
    /// Gets or sets the raw response string representation.
    /// </summary>
    public string RawResponse { get; internal set; }

    /// <summary>
    /// Gets the response headers.
    /// </summary>
    public HttpResponseHeaders Headers => InnerResponse.Headers;

    /// <summary>
    /// Gets the content headers (e.g., Content-Type, Content-Length).
    /// </summary>
    public HttpContentHeaders ContentHeaders => InnerResponse.Content.Headers;

    /// <summary>
    /// Gets the response content as a string.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets the Content-Type media type (e.g., "application/json").
    /// </summary>
    public string? ContentType => ContentHeaders.ContentType?.MediaType;

    /// <summary>
    /// Gets the Content-Length, or null if not specified.
    /// </summary>
    public long? ContentLength => ContentHeaders.ContentLength;

    /// <summary>
    /// Gets the cookies received in the response.
    /// </summary>
    public IReadOnlyList<Cookie> Cookies => _cookies;

    /// <summary>
    /// Gets the HTTP status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode => InnerResponse.StatusCode;

    /// <summary>
    /// Gets the HTTP status reason phrase (e.g., "OK", "Not Found").
    /// </summary>
    public string? ReasonPhrase => InnerResponse.ReasonPhrase;

    /// <summary>
    /// Gets a value indicating whether the response was successful (status code 2xx).
    /// </summary>
    public bool IsSuccessful => InnerResponse.IsSuccessStatusCode;

    /// <summary>
    /// Gets the HTTP version of the response.
    /// </summary>
    public Version Version => InnerResponse.Version;

    /// <summary>
    /// Gets the final request URI (reflects any redirects that occurred).
    /// </summary>
    public Uri? RequestUri => _request.RequestUri;

    /// <summary>
    /// Deserializes the response content into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the content into.</typeparam>
    /// <returns>The deserialized object, or default if content is empty.</returns>
    /// <exception cref="FluentHttpSerializationException">Thrown when deserialization fails.</exception>
    public T? ContentAs<T>()
    {
        if (string.IsNullOrEmpty(Content))
            return default;

        try
        {
            return _serializerProvider.Deserialize<T>(Content);
        }
        catch (Exception ex)
        {
            throw FluentHttpSerializationException.ForDeserialization<T>(Content, this, ex);
        }
    }

    /// <summary>
    /// Attempts to deserialize the response content into the specified type without throwing an exception.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the content into.</typeparam>
    /// <param name="result">When this method returns, contains the deserialized object if successful; otherwise, the default value.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    public bool TryContentAs<T>(out T? result)
    {
        result = default;

        if (string.IsNullOrEmpty(Content))
            return false;

        try
        {
            result = _serializerProvider.Deserialize<T>(Content);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string DecodeContent(byte[] bytes, MediaTypeHeaderValue? contentType)
    {
        if (bytes.Length == 0)
            return string.Empty;

        // Get encoding from Content-Type header, default to UTF-8
        var encoding = System.Text.Encoding.UTF8;

        if (!string.IsNullOrEmpty(contentType?.CharSet))
        {
            try
            {
                encoding = System.Text.Encoding.GetEncoding(contentType.CharSet.Trim('"'));
            }
            catch (ArgumentException)
            {
                // Fall back to UTF-8 for invalid/unknown charsets
            }
        }

        return encoding.GetString(bytes);
    }
}
