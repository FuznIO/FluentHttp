using Microsoft.Extensions.DependencyInjection;

namespace Fuzn.FluentHttp;

/// <summary>
/// Extension methods for registering FluentHttp services.
/// </summary>
public static class FluentHttpServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="FluentHttpSettings"/> as a singleton service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional action to configure the settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFluentHttp(
        this IServiceCollection services,
        Action<FluentHttpSettings>? configure = null)
    {
        var settings = new FluentHttpSettings();
        configure?.Invoke(settings);
        services.AddSingleton(settings);
        return services;
    }
}
