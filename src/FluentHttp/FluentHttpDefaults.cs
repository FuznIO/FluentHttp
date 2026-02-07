namespace Fuzn.FluentHttp;

/// <summary>
/// Global defaults for FluentHttp requests.
/// For dependency injection scenarios, register <see cref="FluentHttpSettings"/> directly
/// and use <see cref="FluentHttpRequest.WithSettings"/> instead.
/// </summary>
public static class FluentHttpDefaults
{
    /// <summary>
    /// Gets or sets the global settings applied to all requests unless overridden
    /// per-request via <see cref="FluentHttpRequest.WithSettings"/> or per-property methods.
    /// </summary>
    public static FluentHttpSettings Settings { get; set; } = new();
}
