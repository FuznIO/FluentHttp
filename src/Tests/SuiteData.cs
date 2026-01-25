using Microsoft.AspNetCore.Mvc.Testing;

namespace Fuzn.FluentHttp.Tests;

internal static class SuiteData
{
    internal static WebApplicationFactory<Program> Factory { get; set; }

    public static void Init()
    {
        Factory = new WebApplicationFactory<Program>();
        Factory.ClientOptions.BaseAddress = new Uri("https://localhost:60201/");
    }
}
