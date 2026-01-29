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

        // Decode the body using the encoding specified in Content-Type header
        Body = DecodeBody(rawBytes, response.Content.Headers.ContentType);

        if (cookieContainer != null)
        {
            var cookies = cookieContainer.GetAllCookies();
            foreach (var cookie in cookies.Cast<Cookie>())
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
        Body = other.Body;
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
    /// Gets the response body as a string.
    /// </summary>
    public string Body { get; }

    /// <summary>
    /// Gets the cookies received in the response.
    /// </summary>
    public IReadOnlyList<Cookie> Cookies => _cookies;

    /// <summary>
    /// Gets the HTTP status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode => InnerResponse.StatusCode;

    /// <summary>
    /// Gets a value indicating whether the response was successful (status code 2xx).
    /// </summary>
    public bool IsSuccessful => InnerResponse.IsSuccessStatusCode;

    /// <summary>
    /// Gets the response body as a byte array.
    /// </summary>
    /// <returns>A byte array containing the response body.</returns>
    public byte[] AsBytes()
    {
        return _rawBytes;
    }

    /// <summary>
    /// Deserializes the response body into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the body into.</typeparam>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="Exception">Thrown when the body is empty, null, or cannot be deserialized.</exception>
    public T? As<T>()
    {
        if (string.IsNullOrEmpty(Body))
            return default;

        try
        {
            var obj = _serializerProvider.Deserialize<T>(Body);

            return obj;
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to deserialize into {typeof(T)}. \nURL: {_request.RequestUri} \nResponse body: \n{Body}\nException message: \n{ex.Message}");
        }
    }

    private static string DecodeBody(byte[] bytes, MediaTypeHeaderValue? contentType)
    {
        if (bytes.Length == 0)
            return string.Empty;

        // Get encoding from Content-Type header, default to UTF-8
        var encoding = contentType?.CharSet != null
            ? System.Text.Encoding.GetEncoding(contentType.CharSet)
            : System.Text.Encoding.UTF8;

        return encoding.GetString(bytes);
    }
}
