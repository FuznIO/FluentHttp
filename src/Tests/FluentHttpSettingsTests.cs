using System.Text.Json;
using Fuzn.FluentHttp;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
[DoNotParallelize]
public class FluentHttpSettingsTests : Test
{
    [Test]
    public async Task GlobalSettings_SetsDefaultSerializerViaRegistry()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.Serializers.Register("application/json", new SystemTextJsonSerializerProvider(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
                return Task.CompletedTask;
            })
            .Step("Global registry serializer applies to request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var payload = new { TestProperty = "value" };

                var response = await client.Url("/api/echo")
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                Assert.Contains("testProperty", response.Content);
            })
            .AfterScenario(_ =>
            {
                FluentHttpDefaults.Serializers.Clear();
                return Task.CompletedTask;
            })
            .Run();
    }

    [Test]
    public async Task PerRequestSerializer_TakesPrecedence_OverGlobalRegistry()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.Serializers.Register("application/json", new SystemTextJsonSerializerProvider(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
                return Task.CompletedTask;
            })
            .Step("Per-request WithSerializer overrides global registry", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var perRequestSerializer = new SystemTextJsonSerializerProvider(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null // PascalCase
                });

                var payload = new { TestProperty = "value" };

                var response = await client.Url("/api/echo")
                    .WithSerializer(perRequestSerializer)
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                Assert.Contains("TestProperty", response.Content);
            })
            .AfterScenario(_ =>
            {
                FluentHttpDefaults.Serializers.Clear();
                return Task.CompletedTask;
            })
            .Run();
    }

    [Test]
    public async Task PerRequestRegistry_TakesPrecedence_OverGlobalRegistry()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.Serializers.Register("application/json", new SystemTextJsonSerializerProvider(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null // PascalCase
                }));
                return Task.CompletedTask;
            })
            .Step("Per-request registry overrides global registry", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var payload = new { TestProperty = "value" };

                var response = await client.Url("/api/echo")
                    .WithSerializer("application/json", new CustomJsonSerializerProvider())
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // CustomJsonSerializerProvider uses camelCase
                Assert.Contains("testProperty", response.Content);
            })
            .AfterScenario(_ =>
            {
                FluentHttpDefaults.Serializers.Clear();
                return Task.CompletedTask;
            })
            .Run();
    }

    [Test]
    public async Task GlobalDefaultSerializer_IsUsedWhenNoContentTypeMatch()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.Serializers.Default = new SystemTextJsonSerializerProvider(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null // PascalCase
                });
                return Task.CompletedTask;
            })
            .Step("Custom default serializer is used as fallback", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var payload = new { TestProperty = "value" };

                var response = await client.Url("/api/echo")
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // Default uses PascalCase, so TestProperty stays as-is
                Assert.Contains("TestProperty", response.Content);
            })
            .AfterScenario(_ =>
            {
                FluentHttpDefaults.Serializers.Clear();
                return Task.CompletedTask;
            })
            .Run();
    }

    [Test]
    public async Task DefaultSettings_WorksNormally()
    {
        await Scenario()
            .Step("Request works normally with default settings", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/status/ok")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }
}
