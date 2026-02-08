using System.Text.Json;
using Fuzn.FluentHttp;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
[DoNotParallelize]
public class FluentHttpDefaultsTests : Test
{
    [Test]
    public async Task GlobalSettings_SetsDefaultSerializerOptions()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.JsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                return Task.CompletedTask;
            })
            .Step("Global settings apply serializer options", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var payload = new { TestProperty = "value" };

                var response = await client.Url("/api/echo")
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // With CamelCase policy, TestProperty becomes testProperty
                Assert.Contains("testProperty", response.Content);
            })
            .AfterScenario(_ =>
            {
                FluentHttpDefaults.JsonOptions = null;
                FluentHttpDefaults.Serializer = null;
                return Task.CompletedTask;
            })
            .Run();
    }

    [Test]
    public async Task PerRequestOptions_TakesPrecedence_OverGlobalSettings()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.JsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                return Task.CompletedTask;
            })
            .Step("Per-request options override global settings", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var perRequestOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null // PascalCase
                };

                var payload = new { TestProperty = "value" };

                var response = await client.Url("/api/echo")
                    .WithJsonOptions(perRequestOptions)
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // Per-request uses PascalCase, so TestProperty stays as-is
                Assert.Contains("TestProperty", response.Content);
            })
            .AfterScenario(_ =>
            {
                FluentHttpDefaults.JsonOptions = null;
                FluentHttpDefaults.Serializer = null;
                return Task.CompletedTask;
            })
            .Run();
    }

    [Test]
    public async Task GlobalSerializer_TakesPrecedence_OverJsonOptions()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.JsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null // PascalCase
                };
                FluentHttpDefaults.Serializer = new CustomJsonSerializerProvider(); // Uses camelCase
                return Task.CompletedTask;
            })
            .Step("Global Serializer takes precedence over JsonOptions", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var payload = new { TestProperty = "value" };

                var response = await client.Url("/api/echo")
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // Serializer uses camelCase, so testProperty
                Assert.Contains("testProperty", response.Content);
            })
            .AfterScenario(_ =>
            {
                FluentHttpDefaults.JsonOptions = null;
                FluentHttpDefaults.Serializer = null;
                return Task.CompletedTask;
            })
            .Run();
    }

    [Test]
    public async Task DefaultSettings_WorksNormally()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.JsonOptions = null;
                FluentHttpDefaults.Serializer = null;
                return Task.CompletedTask;
            })
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
