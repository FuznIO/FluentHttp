using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class ContentTypeTests : Test
{
    [Test]
    public async Task WithContentType_Json_SerializesBodyCorrectly()
    {
        await Scenario()
            .Step("Send JSON body", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var payload = new { name = "Test", value = 123 };
                
                var response = await client.Url("/api/content/json")
                    .WithContentType(ContentTypes.Json)
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<ContentTypeResponse>();
                Assert.AreEqual("application/json", body!.ContentType);
            })
            .Run();
    }

    [Test]
    public async Task WithContentType_JsonAutoSet_WhenBodyProvided()
    {
        await Scenario()
            .Step("Body auto-sets Content-Type to JSON", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var payload = new { name = "Auto" };
                
                var response = await client.Url("/api/echo")
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<ContentTypeResponse>();
                Assert.Contains("application/json", body!.ContentType);
            })
            .Run();
    }

    [Test]
    public async Task WithContentType_CustomString_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with custom content type string", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/echo")
                    .WithContentType("application/xml")
                    .WithContent("<root><test>value</test></root>")
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<ContentTypeResponse>();
                Assert.Contains("application/xml", body!.ContentType);
            })
            .Run();
    }
}
