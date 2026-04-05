namespace Fuzn.FluentHttp;

/// <summary>
/// Global defaults for FluentHttp requests.
/// These settings apply to all requests unless overridden per-request.
/// </summary>
public static class FluentHttpDefaults
{
    /// <summary>
    /// Gets the global serializer registry for content-type-based serializer resolution.
    /// Register serializers for specific content types (e.g., "application/json", "application/xml").
    /// </summary>
    public static SerializerRegistry Serializers { get; } = new();
}
