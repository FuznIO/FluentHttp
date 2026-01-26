using Microsoft.Extensions.DependencyInjection;

namespace Fuzn.FluentHttp.Tests;

internal static class SuiteData
{
    internal static IHttpClientFactory HttpClientFactory { get; set; } = null!;

    public static void Init()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("", client =>
        {
            client.BaseAddress = new Uri("https://localhost:5201/");
        });
        var serviceProvider = services.BuildServiceProvider();
        HttpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    }
}
