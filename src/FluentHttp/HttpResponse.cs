using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Fuzn.FluentHttp;

/// <summary>
/// Represents an HTTP response received from executing an HTTP request.
/// </summary>
public class HttpResponse : IDisposable
{
    private readonly List<Cookie>? _cookies;
    private readonly ISerializerProvider _serializer;
    private readonly byte[] _rawBytes;
    private int _disposed;

    internal HttpResponse(HttpRequestMessage request,
        HttpResponseMessage response,
        CookieContainer? cookieContainer,
        byte[] rawBytes,
        ISerializerProvider serializer)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(rawBytes);
        ArgumentNullException.ThrowIfNull(serializer);

        RequestMessage = request;
        _rawBytes = rawBytes;
        _serializer = serializer;
        InnerResponse = response;

        // Decode the content using the encoding specified in Content-Type header
        Content = DecodeContent(rawBytes, response.Content.Headers.ContentType);

        if (cookieContainer != null)
        {
            var allCookies = cookieContainer.GetAllCookies();
            if (allCookies.Count > 0)
            {
                _cookies = new List<Cookie>(allCookies.Count);
                foreach (Cookie cookie in allCookies)
                {
                    _cookies.Add(cookie);
                }
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

        RequestMessage = other.RequestMessage;
        _rawBytes = other._rawBytes;
        _serializer = other._serializer;
        _cookies = other._cookies;
        InnerResponse = other.InnerResponse;
        Content = other.Content;
    }

    /// <summary>
    /// Gets the underlying <see cref="HttpResponseMessage"/>.
    /// </summary>
    public HttpResponseMessage InnerResponse { get; }

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
    public IReadOnlyList<Cookie> Cookies => (IReadOnlyList<Cookie>?)_cookies ?? Array.Empty<Cookie>();

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
    /// Gets the HTTP request message that was sent to receive this response.
    /// </summary>
    public HttpRequestMessage RequestMessage { get; }

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
            return _serializer.Deserialize<T>(Content);
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
            result = _serializer.Deserialize<T>(Content);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns a formatted debug string showing the HTTP response details.
    /// Useful for logging and debugging.
    /// </summary>
    /// <returns>A formatted string containing the response details.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== FluentHttp Response ===");
        sb.AppendLine($"Status: {(int)StatusCode} {ReasonPhrase}");
        sb.AppendLine($"Success: {IsSuccessful}");
        sb.AppendLine($"Version: HTTP/{Version}");

        if (Headers.Any())
        {
            sb.AppendLine("Headers:");
            foreach (var header in Headers)
                sb.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        }

        if (ContentHeaders.Any())
        {
            sb.AppendLine("Content Headers:");
            foreach (var header in ContentHeaders)
                sb.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        }

        if (_cookies?.Count > 0)
        {
            sb.AppendLine($"Cookies: {_cookies.Count}");
            foreach (var cookie in _cookies)
                sb.AppendLine($"  {cookie.Name} = {cookie.Value}");
        }

        if (!string.IsNullOrEmpty(Content))
        {
            sb.AppendLine($"Content: {Content}");
        }
        else
        {
            sb.AppendLine("Content: (empty)");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Releases all resources used by the <see cref="HttpResponse"/>.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        InnerResponse.Dispose();
        RequestMessage.Dispose();
        GC.SuppressFinalize(this);
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
