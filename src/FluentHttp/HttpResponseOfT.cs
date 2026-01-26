using System.Net;

namespace Fuzn.FluentHttp;

/// <summary>
/// Represents a typed HTTP response with deserialized data.
/// </summary>
/// <typeparam name="T">The type of the deserialized response data.</typeparam>
public class HttpResponse<T> : HttpResponse
{
    private readonly Lazy<T?> _data;

    internal HttpResponse(HttpResponse response)
        : base(response)
    {
        _data = new Lazy<T?>(As<T>);
    }

    /// <summary>
    /// Gets the deserialized response data. Returns default(T) if the body is empty or cannot be deserialized.
    /// </summary>
    public T? Data => _data.Value;
}
