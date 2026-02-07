using System.Text.Json;

namespace Fuzn.FluentHttp;

/// <summary>
/// Configuration settings for FluentHttp requests.
/// These settings can be applied globally via <see cref="FluentHttpDefaults.Settings"/>,
/// per-client via dependency injection, or per-request via <see cref="FluentHttpRequest.WithSettings"/>.
/// </summary>
public class FluentHttpSettings
{
    /// <summary>
    /// Gets or sets the default JSON serializer options.
    /// Ignored if <see cref="Serializer"/> is set.
    /// </summary>
    public JsonSerializerOptions? JsonOptions { get; set; }

    /// <summary>
    /// Gets or sets the custom serializer provider.
    /// Takes precedence over <see cref="JsonOptions"/>.
    /// </summary>
    public ISerializerProvider? Serializer { get; set; }
}
