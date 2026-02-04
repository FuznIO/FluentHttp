using System.Net;

namespace Fuzn.FluentHttp;

/// <summary>
/// Represents a typed HTTP response with deserialized data.
/// </summary>
/// <typeparam name="T">The type of the deserialized response data.</typeparam>
public class FluentHttpResponse<T> : FluentHttpResponse
{
    private readonly Lazy<T?> _data;

    internal FluentHttpResponse(FluentHttpResponse response)
        : base(response)
    {
        _data = new Lazy<T?>(ContentAs<T>);
    }

    /// <summary>
    /// Gets the deserialized response data. Returns default(T) if the content is empty or cannot be deserialized.
    /// </summary>
    public T? Data => _data.Value;
}
