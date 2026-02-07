using System.Text.Json;
using Fuzn.FluentHttp;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
[DoNotParallelize]
public class FluentHttpSettingsTests : Test
{
    [Test]
    public async Task GlobalSettings_SetsDefaultSerializerOptions()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.Settings = new FluentHttpSettings
                {
                    JsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }
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
                FluentHttpDefaults.Settings = new FluentHttpSettings();
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
                FluentHttpDefaults.Settings = new FluentHttpSettings
                {
                    JsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }
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
                FluentHttpDefaults.Settings = new FluentHttpSettings();
                return Task.CompletedTask;
            })
            .Run();
    }

    [Test]
    public async Task InstanceSettings_TakesPrecedence_OverGlobalSettings()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.Settings = new FluentHttpSettings
                {
                    JsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }
                };
                return Task.CompletedTask;
            })
            .Step("Instance settings override global settings", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var instanceSettings = new FluentHttpSettings
                {
                    JsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = null // PascalCase
                    }
                };

                var payload = new { TestProperty = "value" };

                var response = await client.Url("/api/echo")
                    .WithSettings(instanceSettings)
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // Instance settings use PascalCase, so TestProperty stays as-is
                Assert.Contains("TestProperty", response.Content);
            })
            .AfterScenario(_ =>
            {
                FluentHttpDefaults.Settings = new FluentHttpSettings();
                return Task.CompletedTask;
            })
            .Run();
    }

    [Test]
    public async Task PerRequestOptions_TakesPrecedence_OverInstanceSettings()
    {
        await Scenario()
            .Step("Per-request options override instance settings", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var instanceSettings = new FluentHttpSettings
                {
                    JsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }
                };

                var perRequestOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null // PascalCase
                };

                var payload = new { TestProperty = "value" };

                var response = await client.Url("/api/echo")
                    .WithSettings(instanceSettings)
                    .WithJsonOptions(perRequestOptions)
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // Per-request uses PascalCase, so TestProperty stays as-is
                Assert.Contains("TestProperty", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task DefaultSettings_WorksNormally()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.Settings = new FluentHttpSettings();
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
