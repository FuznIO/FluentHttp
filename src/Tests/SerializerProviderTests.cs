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
                var client = SuiteData.Factory.CreateClient();
                var customSerializer = new CustomJsonSerializerProvider();
                
                var response = await client.Url("/api/deserialize/person")
                    .SerializerProvider(customSerializer)
                    .Get();

                Assert.IsTrue(response.Ok);
                
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
                var client = SuiteData.Factory.CreateClient();
                
                var payload = new { TestProperty = "value" };
                
                var response = await client.Url("/api/echo")
                    .Body(payload)
                    .Post();

                Assert.IsTrue(response.Ok);
                // The default serializer preserves property names as-is
                Assert.Contains("TestProperty", response.Body);
            })
            .Run();
    }
}
