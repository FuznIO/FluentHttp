using System.Text.Json;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
[DoNotParallelize]
public class FluentHttpDefaultsTests : Test
{
    [Test]
    public async Task BeforeSend_SetsDefaultSerializerOptions_WhenNotSetPerRequest()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                var globalOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                FluentHttpDefaults.BeforeSend = builder =>
                {
                    if (builder.Data.JsonOptions is null)
                    {
                        builder.WithJsonOptions(globalOptions);
                    }
                };
                return Task.CompletedTask;
            })
            .Step("BeforeSend sets default serializer options", async _ =>
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
                FluentHttpDefaults.BeforeSend = null;
                return Task.CompletedTask;
            })
            .Run();
    }

    [Test]
    public async Task BeforeSend_DoesNotOverride_WhenSerializerSetPerRequest()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                var globalOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                FluentHttpDefaults.BeforeSend = builder =>
                {
                    if (builder.Data.JsonOptions is null)
                    {
                        builder.WithJsonOptions(globalOptions);
                    }
                };
                return Task.CompletedTask;
            })
            .Step("Per-request serializer takes precedence over BeforeSend", async _ =>
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
                FluentHttpDefaults.BeforeSend = null;
                return Task.CompletedTask;
            })
            .Run();
    }

    [Test]
    public async Task BeforeSend_AddsDefaultHeader_WhenNotPresent()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.BeforeSend = builder =>
                {
                    if (!builder.Data.Headers.ContainsKey("X-Custom-Header"))
                    {
                        builder.WithHeader("X-Custom-Header", "GlobalDefault");
                    }
                };
            })
            .Step("BeforeSend adds header if not already set", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/headers/echo")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.Contains("GlobalDefault", response.Content);
            })
            .AfterScenario(_ =>
            {
                FluentHttpDefaults.BeforeSend = null;
            })
            .Run();
    }

    [Test]
    public async Task BeforeSend_DoesNotOverrideHeader_WhenSetPerRequest()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.BeforeSend = builder =>
                {
                    if (!builder.Data.Headers.ContainsKey("X-Custom-Header"))
                    {
                        builder.WithHeader("X-Custom-Header", "GlobalDefault");
                    }
                };
            })
            .Step("Per-request header takes precedence", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/headers/echo")
                    .WithHeader("X-Custom-Header", "PerRequestValue")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.Contains("PerRequestValue", response.Content);
                Assert.DoesNotContain("GlobalDefault", response.Content);
            })
            .AfterScenario(_ =>
            {
                FluentHttpDefaults.BeforeSend = null;
            })
            .Run();
    }

    [Test]
    public async Task BeforeSend_CanInspectRequestUrl_ForConditionalLogic()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.BeforeSend = builder =>
                {
                    // Add header only for specific endpoints
                    if (builder.Data.RequestUrl.Contains("/api/echo"))
                    {
                        builder.WithHeader("X-Echo-Request", "true");
                    }
                };
            })
            .Step("BeforeSend can check URL for conditional behavior", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/echo")
                    .WithContent(new { test = "value" })
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
            })
            .AfterScenario(_ =>
            {
                FluentHttpDefaults.BeforeSend = null;
            })
            .Run();
    }

    [Test]
    public async Task BeforeSend_WhenNull_NoInterceptorRuns()
    {
        await Scenario()
            .BeforeScenario(_ =>
            {
                FluentHttpDefaults.BeforeSend = null;
            })
            .Step("Request works normally when BeforeSend is null", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/status/ok")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .AfterScenario(_ =>
            {
                FluentHttpDefaults.BeforeSend = null;
            })
            .Run();
    }
}
