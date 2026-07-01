using Microsoft.Extensions.DependencyInjection;

namespace Fuzn.FluentHttp.Testing;

/// <summary>
/// Extension methods for wiring a <see cref="MockHttpHandler"/> into an
/// <see cref="IHttpClientFactory"/>-based setup (named or typed clients).
/// </summary>
public static class MockHttpHandlerHttpClientBuilderExtensions
{
    /// <summary>
    /// Registers <paramref name="handler"/> as the primary (terminal) HTTP message handler for the
    /// named or typed client, so its requests are served by the mock instead of the network. Any
    /// <see cref="DelegatingHandler"/>s configured on the client continue to run in front of the mock.
    /// </summary>
    /// <param name="builder">The HttpClient builder returned by <c>AddHttpClient</c>.</param>
    /// <param name="handler">The mock handler that should serve the client's requests.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static IHttpClientBuilder UseMockHandler(this IHttpClientBuilder builder, MockHttpHandler handler)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(handler);

        return builder.ConfigurePrimaryHttpMessageHandler(() => handler);
    }
}
