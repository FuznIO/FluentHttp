using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class ContentTypesEnumTests : Test
{
    [Test]
    public async Task ContentType_XmlEnum_SetsCorrectContentType()
    {
        await Scenario()
            .Step("Send request with ContentType Xml enum", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/echo")
                    .ContentType(ContentTypes.Xml)
                    .Body("<root><value>test</value></root>")
                    .Post();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<ContentTypeResponse>();
                Assert.Contains("application/xml", body!.ContentType);
            })
            .Run();
    }

    [Test]
    public async Task ContentType_PlainTextEnum_SetsCorrectContentType()
    {
        await Scenario()
            .Step("Send request with ContentType PlainText enum", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/echo")
                    .ContentType(ContentTypes.PlainText)
                    .Body("Plain text content")
                    .Post();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<ContentTypeResponse>();
                Assert.Contains("text/plain", body!.ContentType);
            })
            .Run();
    }

    [Test]
    public async Task ContentType_OctetStreamEnum_SetsCorrectContentType()
    {
        await Scenario()
            .Step("Send request with ContentType OctetStream enum", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var binaryContent = System.Text.Encoding.UTF8.GetBytes("Binary-like content");
                
                var response = await client.Url("/api/echo")
                    .ContentType(ContentTypes.OctetStream)
                    .Body(binaryContent)
                    .Post();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<ContentTypeResponse>();
                Assert.Contains("application/octet-stream", body!.ContentType);
            })
            .Run();
    }

    [Test]
    public async Task ContentType_XFormUrlEncodedEnum_SetsCorrectContentType()
    {
        await Scenario()
            .Step("Send request with ContentType XFormUrlEncoded enum", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var formData = new Dictionary<string, string>
                {
                    ["key"] = "value",
                    ["another"] = "data"
                };
                
                var response = await client.Url("/api/echo")
                    .ContentType(ContentTypes.XFormUrlEncoded)
                    .Body(formData)
                    .Post();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<ContentTypeResponse>();
                Assert.Contains("application/x-www-form-urlencoded", body!.ContentType);
            })
            .Run();
    }

    [Test]
    public async Task AsMultipart_SetsMultipartContentType()
    {
        await Scenario()
            .Step("AsMultipart sets content type to multipart/form-data", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/files/upload")
                    .AsMultipart()
                    .FormField("field1", "value1")
                    .Post();

                Assert.IsTrue(response.Ok);
            })
            .Run();
    }
}
