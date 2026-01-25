using System.Net;
using System.Net.Http.Headers;
using Fuzn.FluentHttp.Internals;

namespace Fuzn.FluentHttp;

/// <summary>
/// Represents an HTTP response received from executing an HTTP request.
/// </summary>
public class HttpResponse
{
    private readonly List<Cookie> _cookies = new();
    private readonly HttpRequestMessage _request;
    private readonly ISerializerProvider _serializerProvider;

    internal HttpResponse(HttpRequestMessage request,
        HttpResponseMessage response,
        CookieContainer? cookieContainer,
        string body,
        ISerializerProvider serializerProvider)
    {
        _request = request;
        _serializerProvider = serializerProvider;
        InnerResponse = response;
        RawResponse = response.ToString();
        Body = body;

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
    /// Gets the underlying <see cref="HttpResponseMessage"/>.
    /// </summary>
    public HttpResponseMessage InnerResponse { get; }

    /// <summary>
    /// Gets or sets the raw response string representation.
    /// </summary>
    public string RawResponse { get; set; }


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
    public List<Cookie> Cookies => _cookies;

    /// <summary>
    /// Gets the HTTP status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode => InnerResponse.StatusCode;

    /// <summary>
    /// Gets a value indicating whether the response was successful (status code 2xx).
    /// </summary>
    public bool Ok => InnerResponse.IsSuccessStatusCode;

    /// <summary>
    /// Gets the response body as a byte array.
    /// </summary>
    /// <returns>A byte array containing the response body.</returns>
    public byte[] BodyAsBytes()
    {
        if (string.IsNullOrEmpty(Body))
            return [];

        return System.Text.Encoding.UTF8.GetBytes(Body);
    }

    /// <summary>
    /// Deserializes the response body into the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the body into.</typeparam>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="Exception">Thrown when the body is empty, null, or cannot be deserialized.</exception>
    public T? BodyAs<T>()
    {
        if (string.IsNullOrEmpty(Body))
            return default;

        try
        {
            var obj = _serializerProvider.Deserialize<T>(Body);
            if (obj == null)
                throw new Exception($"Deserialized object is null.");
            return obj;
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to deserialize into {typeof(T)}. \nURL: {_request?.RequestUri} \nResponse body: \n{Body}\nException message: \n{ex?.Message}");
        }
    }

    /// <summary>
    /// Parses the response body as a dynamic JSON object.
    /// </summary>
    /// <returns>A dynamic object representing the JSON response, or null if the body is null.</returns>
    /// <exception cref="Exception">Thrown when the body is not valid JSON.</exception>
    public dynamic? BodyAsJson()
    {
        if (Body == null)
            return null;

        try
        {
            dynamic? json = DynamicHelper.ParseJsonToDynamic(Body);
            return json;
        }
        catch (Exception)
        {
            throw new Exception($"The response body was not a valid JSON. \nURL: {_request.RequestUri} \nResponse body: \n{Body}");
        }
    }
}
