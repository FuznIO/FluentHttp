namespace Fuzn.FluentHttp.Internals;

/// <summary>
/// Encapsulates the serializer resolution chain for both request and response.
/// Passed to FluentHttpResponse so it can resolve by response Content-Type at deserialization time.
/// </summary>
internal class SerializerResolver
{
    private readonly ISerializerProvider? _perRequestSerializer;
    private readonly SerializerRegistry? _perRequestRegistry;

    internal SerializerResolver(
        ISerializerProvider? perRequestSerializer,
        SerializerRegistry? perRequestRegistry)
    {
        _perRequestSerializer = perRequestSerializer;
        _perRequestRegistry = perRequestRegistry;
    }

    /// <summary>
    /// Resolve serializer for request serialization (uses request content type).
    /// </summary>
    internal ISerializerProvider ResolveForRequest(string? contentType)
    {
        return Resolve(contentType);
    }

    /// <summary>
    /// Resolve serializer for response deserialization (uses response content type).
    /// </summary>
    internal ISerializerProvider ResolveForResponse(string? responseContentType)
    {
        return Resolve(responseContentType);
    }

    private ISerializerProvider Resolve(string? contentType)
    {
        // 1. Per-request escape hatch
        if (_perRequestSerializer is not null)
            return _perRequestSerializer;

        // 2. Per-request registry
        if (_perRequestRegistry is not null)
        {
            var fromRegistry = _perRequestRegistry.Resolve(contentType);
            if (fromRegistry is not null)
                return fromRegistry;
        }

        // 3. Global registry
        var fromGlobal = FluentHttpDefaults.Serializers.Resolve(contentType);
        if (fromGlobal is not null)
            return fromGlobal;

        // 4. Default
        return FluentHttpDefaults.Serializers.Default;
    }
}
