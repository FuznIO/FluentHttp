namespace Fuzn.FluentHttp;

/// <summary>
/// Exception thrown when serialization or deserialization of HTTP content fails.
/// </summary>
public class FluentHttpSerializationException : Exception
{
    /// <summary>
    /// Gets the raw content that failed to serialize or deserialize.
    /// </summary>
    public string? Content { get; }

    /// <summary>
    /// Gets the target type for deserialization, or source type for serialization.
    /// </summary>
    public Type? TargetType { get; }

    /// <summary>
    /// Gets the HTTP response associated with the failure, if available.
    /// </summary>
    public HttpResponse? Response { get; }

    internal FluentHttpSerializationException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    internal FluentHttpSerializationException(
        string message,
        string? content,
        Type? targetType,
        HttpResponse? response,
        Exception? innerException)
        : base(message, innerException)
    {
        Content = content;
        TargetType = targetType;
        Response = response;
    }

    internal static FluentHttpSerializationException ForSerialization<T>(T? obj, Exception innerException) =>
        new(
            $"Failed to serialize object of type {typeof(T).Name}.",
            obj?.ToString(),
            typeof(T),
            null,
            innerException);

    internal static FluentHttpSerializationException ForDeserialization<T>(
        string content,
        HttpResponse? response,
        Exception innerException) =>
        new(
            $"Failed to deserialize response content into {typeof(T).Name}.",
            content,
            typeof(T),
            response,
            innerException);
}
