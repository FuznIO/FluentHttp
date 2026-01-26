using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class ContentTypeTests : Test
{
    [Test]
    public async Task ContentType_Json_SerializesBodyCorrectly()
    {
        await Scenario()
            .Step("Send JSON body", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var payload = new { name = "Test", value = 123 };
                
                var response = await client.Url("/api/content/json")
                    .ContentType(ContentTypes.Json)
                    .Body(payload)
                    .Post();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<ContentTypeResponse>();
                Assert.AreEqual("application/json", body!.ContentType);
            })
            .Run();
    }

    [Test]
    public async Task ContentType_JsonAutoSet_WhenBodyProvided()
    {
        await Scenario()
            .Step("Body auto-sets Content-Type to JSON", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var payload = new { name = "Auto" };
                
                var response = await client.Url("/api/echo")
                    .Body(payload)
                    .Post();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<ContentTypeResponse>();
                Assert.Contains("application/json", body!.ContentType);
            })
            .Run();
    }

    [Test]
    public async Task ContentType_CustomString_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with custom content type string", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/echo")
                    .ContentType("application/xml")
                    .Body("<root><test>value</test></root>")
                    .Post();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<ContentTypeResponse>();
                Assert.Contains("application/xml", body!.ContentType);
            })
            .Run();
    }
}
