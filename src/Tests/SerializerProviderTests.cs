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

    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _options);
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
    public async Task WithSerializer_DefaultSerializer_SerializesWithPascalCase()
    {
        await Scenario()
            .Step("Default serializer uses default naming policy", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var payload = new { TestProperty = "value" };
                
                var response = await client.Url("/api/echo")
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // The default serializer preserves property names as-is
                Assert.Contains("TestProperty", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task WithJsonOptions_CamelCase_SerializesWithCamelCase()
    {
        await Scenario()
            .Step("WithJsonOptions with CamelCase naming policy serializes properties as camelCase", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var payload = new { TestProperty = "value", AnotherProperty = 123 };

                var response = await client.Url("/api/echo")
                    .WithJsonOptions(options)
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                // With CamelCase policy, TestProperty becomes testProperty
                Assert.Contains("testProperty", response.Content);
                Assert.Contains("anotherProperty", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task WithJsonOptions_CaseInsensitive_DeserializesCorrectly()
    {
        await Scenario()
            .Step("WithJsonOptions with case insensitive option deserializes regardless of casing", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var response = await client.Url("/api/deserialize/person")
                    .WithJsonOptions(options)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);

                var person = response.ContentAs<PersonDto>();
                Assert.IsNotNull(person);
                Assert.AreEqual("John Doe", person!.Name);
            })
            .Run();
    }

    [Test]
    public async Task WithJsonOptions_IgnoredWhenWithSerializerSet()
    {
        await Scenario()
            .Step("WithJsonOptions is ignored when custom WithSerializer is set", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                var customSerializer = new CustomJsonSerializerProvider();

                // WithJsonOptions would use PascalCase, but WithSerializer uses CamelCase
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null // PascalCase (default)
                };

                var payload = new { TestProperty = "value" };

                var response = await client.Url("/api/echo")
                    .WithJsonOptions(options)
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
