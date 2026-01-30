using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class DeserializationEdgeCaseTests : Test
{
    [Test]
    public async Task ContentAs_EmptyContent_ReturnsDefault()
    {
        await Scenario()
            .Step("Empty content returns default value", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/status/nocontent").Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.AreEqual(string.Empty, response.Content);
                
                // Deserializing empty content should return null/default
                var result = response.ContentAs<PersonDto>();
                Assert.IsNull(result);
            })
            .Run();
    }

    [Test]
    public async Task ContentAs_InvalidJson_ThrowsException()
    {
        await Scenario()
            .Step("Invalid JSON throws exception during deserialization", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/response/text").Get();

                Assert.IsTrue(response.IsSuccessful);
                
                // Plain text content cannot be deserialized to a complex object
                var exceptionThrown = false;
                try
                {
                    response.ContentAs<PersonDto>();
                }
                catch (FluentHttpSerializationException ex)
                {
                    exceptionThrown = true;
                    Assert.Contains("Failed to deserialize", ex.Message);
                    Assert.AreEqual(typeof(PersonDto), ex.TargetType);
                    Assert.IsNotNull(ex.Content);
                    Assert.IsNotNull(ex.Response);
                }
                
                Assert.IsTrue(exceptionThrown, "Expected FluentHttpSerializationException when deserializing invalid JSON");
            })
            .Run();
    }

    [Test]
    public async Task ContentAs_NestedObject_DeserializesCorrectly()
    {
        await Scenario()
            .Step("Nested objects deserialize correctly", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/echo")
                    .WithContent(new { 
                        outer = new { 
                            inner = new { 
                                value = "nested" 
                            } 
                        } 
                    })
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                Assert.Contains("nested", response.Content);
            })
            .Run();
    }
}
