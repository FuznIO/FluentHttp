using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class AcceptTypesTests : Test
{
    [Test]
    public async Task WithAccept_XmlEnum_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Accept Xml enum", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .WithAccept(AcceptTypes.Xml)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<AcceptHeaderResponse>();
                Assert.Contains("application/xml", body!.Accept);
            })
            .Run();
    }

    [Test]
    public async Task WithAccept_PlainTextEnum_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Accept PlainText enum", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .WithAccept(AcceptTypes.PlainText)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<AcceptHeaderResponse>();
                Assert.Contains("text/plain", body!.Accept);
            })
            .Run();
    }

    [Test]
    public async Task WithAccept_HtmlEnum_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Accept Html enum", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .WithAccept(AcceptTypes.Html)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<AcceptHeaderResponse>();
                Assert.Contains("text/html", body!.Accept);
            })
            .Run();
    }

    [Test]
    public async Task WithAccept_AnyEnum_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Accept Any enum", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .WithAccept(AcceptTypes.Any)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<AcceptHeaderResponse>();
                Assert.Contains("*/*", body!.Accept);
            })
            .Run();
    }

    [Test]
    public async Task WithAccept_OctetStreamEnum_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Accept OctetStream enum", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .WithAccept(AcceptTypes.OctetStream)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<AcceptHeaderResponse>();
                Assert.Contains("application/octet-stream", body!.Accept);
            })
            .Run();
    }
}
