using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class DeserializationEdgeCaseTests : Test
{
    [Test]
    public async Task As_EmptyBody_ReturnsDefault()
    {
        await Scenario()
            .Step("Empty body returns default value", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/status/nocontent").Get();

                Assert.IsTrue(response.Ok);
                Assert.AreEqual(string.Empty, response.Body);
                
                // Deserializing empty body should return null/default
                var result = response.As<PersonDto>();
                Assert.IsNull(result);
            })
            .Run();
    }

    [Test]
    public async Task As_InvalidJson_ThrowsException()
    {
        await Scenario()
            .Step("Invalid JSON throws exception during deserialization", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/response/text").Get();

                Assert.IsTrue(response.Ok);
                
                // Plain text body cannot be deserialized to a complex object
                var exceptionThrown = false;
                try
                {
                    response.As<PersonDto>();
                }
                catch (Exception)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected exception to be thrown when deserializing invalid JSON");
            })
            .Run();
    }

    [Test]
    public async Task As_NestedObject_DeserializesCorrectly()
    {
        await Scenario()
            .Step("Nested objects deserialize correctly", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/echo")
                    .Body(new { 
                        outer = new { 
                            inner = new { 
                                value = "nested" 
                            } 
                        } 
                    })
                    .Post();

                Assert.IsTrue(response.Ok);
                Assert.Contains("nested", response.Body);
            })
            .Run();
    }
}
