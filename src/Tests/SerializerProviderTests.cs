using Fuzn.FluentHttp.TestApi.Models;
using System.Text.Json;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

/// <summary>
/// Custom serializer for testing the SerializerProvider functionality.
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

    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _options);
    }
}

[TestClass]
public class SerializerProviderTests : Test
{
    [Test]
    public async Task SerializerProvider_CustomProvider_IsUsedForDeserialization()
    {
        await Scenario()
            .Step("Custom serializer provider is used for deserialization", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                var customSerializer = new CustomJsonSerializerProvider();
                
                var response = await client.Url("/api/deserialize/person")
                    .SerializerProvider(customSerializer)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var person = response.As<PersonDto>();
                Assert.IsNotNull(person);
                Assert.AreEqual("John Doe", person!.Name);
            })
            .Run();
    }

    [Test]
    public async Task SerializerProvider_DefaultSerializer_SerializesWithPascalCase()
    {
        await Scenario()
            .Step("Default serializer uses default naming policy", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var payload = new { TestProperty = "value" };
                
                var response = await client.Url("/api/echo")
                    .Body(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // The default serializer preserves property names as-is
                Assert.Contains("TestProperty", response.Body);
            })
            .Run();
    }

    [Test]
    public async Task SerializerOptions_CamelCase_SerializesWithCamelCase()
    {
        await Scenario()
            .Step("SerializerOptions with CamelCase naming policy serializes properties as camelCase", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var payload = new { TestProperty = "value", AnotherProperty = 123 };

                var response = await client.Url("/api/echo")
                    .SerializerOptions(options)
                    .Body(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // With CamelCase policy, TestProperty becomes testProperty
                Assert.Contains("testProperty", response.Body);
                Assert.Contains("anotherProperty", response.Body);
            })
            .Run();
    }

    [Test]
    public async Task SerializerOptions_CaseInsensitive_DeserializesCorrectly()
    {
        await Scenario()
            .Step("SerializerOptions with case insensitive option deserializes regardless of casing", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var response = await client.Url("/api/deserialize/person")
                    .SerializerOptions(options)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);

                var person = response.As<PersonDto>();
                Assert.IsNotNull(person);
                Assert.AreEqual("John Doe", person!.Name);
            })
            .Run();
    }

    [Test]
    public async Task SerializerOptions_IgnoredWhenSerializerProviderSet()
    {
        await Scenario()
            .Step("SerializerOptions is ignored when custom SerializerProvider is set", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                var customSerializer = new CustomJsonSerializerProvider();

                // SerializerOptions would use PascalCase, but SerializerProvider uses CamelCase
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null // PascalCase (default)
                };

                var payload = new { TestProperty = "value" };

                var response = await client.Url("/api/echo")
                    .SerializerOptions(options)
                    .SerializerProvider(customSerializer)
                    .Body(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // SerializerProvider takes precedence, so camelCase is used
                Assert.Contains("testProperty", response.Body);
            })
            .Run();
    }
}
