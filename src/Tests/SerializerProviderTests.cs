using Fuzn.FluentHttp.TestApi.Models;
using System.Text.Json;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

/// <summary>
/// Custom serializer for testing the WithSerializer functionality.
/// </summary>
public class CustomJsonSerializerProvider : ISerializerProvider
{
    private readonly JsonSerializerOptions _options;

    public CustomJsonSerializerProvider()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, _options);
    }

    public T? Deserialize<T>(string content)
    {
        return JsonSerializer.Deserialize<T>(content, _options);
    }
}

[TestClass]
public class SerializerProviderTests : Test
{
    [Test]
    public async Task WithSerializer_CustomProvider_IsUsedForDeserialization()
    {
        await Scenario()
            .Step("Custom serializer provider is used for deserialization", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                var customSerializer = new CustomJsonSerializerProvider();

                var response = await client.Url("/api/deserialize/person")
                    .WithSerializer(customSerializer)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);

                var person = response.ContentAs<PersonDto>();
                Assert.IsNotNull(person);
                Assert.AreEqual("John Doe", person!.Name);
            })
            .Run();
    }

    [Test]
    public async Task WithSerializer_DefaultSerializer_SerializesWithCamelCase()
    {
        await Scenario()
            .Step("Default serializer uses camelCase naming policy", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var payload = new { TestProperty = "value" };

                var response = await client.Url("/api/echo")
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // The default serializer uses JsonSerializerDefaults.Web which applies camelCase
                Assert.Contains("testProperty", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task WithSerializer_ByContentType_SerializesWithCamelCase()
    {
        await Scenario()
            .Step("Per-request registry with CamelCase serializer serializes properties as camelCase", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var payload = new { TestProperty = "value", AnotherProperty = 123 };

                var response = await client.Url("/api/echo")
                    .WithSerializer("application/json", new SystemTextJsonSerializerProvider(options))
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                Assert.Contains("testProperty", response.Content);
                Assert.Contains("anotherProperty", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task WithSerializer_ByContentType_DeserializesCorrectly()
    {
        await Scenario()
            .Step("Per-request registry with case insensitive option deserializes regardless of casing", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var response = await client.Url("/api/deserialize/person")
                    .WithSerializer("application/json", new SystemTextJsonSerializerProvider(options))
                    .Get();

                Assert.IsTrue(response.IsSuccessful);

                var person = response.ContentAs<PersonDto>();
                Assert.IsNotNull(person);
                Assert.AreEqual("John Doe", person!.Name);
            })
            .Run();
    }

    [Test]
    public async Task WithSerializer_OverridesRegistry()
    {
        await Scenario()
            .Step("WithSerializer takes precedence over per-request registry", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                var customSerializer = new CustomJsonSerializerProvider();

                // Registry would use PascalCase, but WithSerializer uses CamelCase
                var pascalOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null // PascalCase
                };

                var payload = new { TestProperty = "value" };

                var response = await client.Url("/api/echo")
                    .WithSerializer("application/json", new SystemTextJsonSerializerProvider(pascalOptions))
                    .WithSerializer(customSerializer)
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // WithSerializer takes precedence, so camelCase is used
                Assert.Contains("testProperty", response.Content);
            })
            .Run();
    }
}
