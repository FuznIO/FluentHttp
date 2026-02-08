using System.Text.Json;

namespace Fuzn.FluentHttp;

/// <summary>
/// Global defaults for FluentHttp requests.
/// These settings apply to all requests unless overridden per-request.
/// </summary>
public static class FluentHttpDefaults
{
    /// <summary>
    /// Gets or sets the default JSON serializer options.
    /// Ignored if <see cref="Serializer"/> is set.
    /// </summary>
    public static JsonSerializerOptions? JsonOptions { get; set; }

    /// <summary>
    /// Gets or sets the custom serializer provider.
    /// Takes precedence over <see cref="JsonOptions"/>.
    /// </summary>
    public static ISerializerProvider? Serializer { get; set; }
}
