namespace Fuzn.FluentHttp;

/// <summary>
/// Maps content type strings to serializer providers.
/// Used for content-type-based serializer resolution at both global and per-request levels.
/// </summary>
public class SerializerRegistry
{
    private readonly Dictionary<string, ISerializerProvider> _serializers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a serializer provider for a content type.
    /// </summary>
    /// <param name="contentType">The content type (e.g., "application/json", "application/xml").</param>
    /// <param name="serializer">The serializer provider to use for this content type.</param>
    /// <returns>The current registry instance for method chaining.</returns>
    public SerializerRegistry Register(string contentType, ISerializerProvider serializer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentNullException.ThrowIfNull(serializer);
        _serializers[contentType] = serializer;
        return this;
    }

    /// <summary>
    /// Attempts to resolve a serializer for the given content type.
    /// Tries exact match first, then strips parameters (e.g., charset) and retries.
    /// </summary>
    /// <param name="contentType">The content type to resolve a serializer for.</param>
    /// <returns>The matching serializer provider, or null if no match is found.</returns>
    internal ISerializerProvider? Resolve(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return null;

        if (_serializers.TryGetValue(contentType, out var serializer))
            return serializer;

        // Try without parameters (e.g., "application/json; charset=utf-8" -> "application/json")
        var semiIndex = contentType.IndexOf(';');
        if (semiIndex > 0)
        {
            var mediaType = contentType[..semiIndex].Trim();
            if (_serializers.TryGetValue(mediaType, out serializer))
                return serializer;
        }

        return null;
    }

    /// <summary>
    /// Resolves a serializer for the given content type, falling back to <see cref="Default"/>
    /// when no content-type-specific serializer is registered.
    /// </summary>
    /// <param name="contentType">The content type to resolve a serializer for.</param>
    /// <returns>The matching serializer provider, or <see cref="Default"/> if none matches.</returns>
    public ISerializerProvider ResolveOrDefault(string? contentType) => Resolve(contentType) ?? Default;

    /// <summary>
    /// Gets the registered content types.
    /// </summary>
    internal IReadOnlyCollection<string> ContentTypes => _serializers.Keys;

    /// <summary>
    /// Gets or sets the default serializer used when no content-type-specific match is found.
    /// Defaults to <see cref="SystemTextJsonSerializerProvider"/> with <see cref="System.Text.Json.JsonSerializerDefaults.Web"/>.
    /// </summary>
    public ISerializerProvider Default { get; set; } = new SystemTextJsonSerializerProvider();

    /// <summary>
    /// Gets a value indicating whether any serializers are registered.
    /// </summary>
    internal bool HasRegistrations => _serializers.Count > 0;

    /// <summary>
    /// Removes all registered serializers and resets the default serializer.
    /// </summary>
    internal void Clear()
    {
        _serializers.Clear();
        Default = new SystemTextJsonSerializerProvider();
    }
}
