using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class AcceptTypesTests : Test
{
    [Test]
    public async Task Accept_XmlEnum_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Accept Xml enum", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .Accept(AcceptTypes.Xml)
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<AcceptHeaderResponse>();
                Assert.Contains("application/xml", body!.Accept);
            })
            .Run();
    }

    [Test]
    public async Task Accept_PlainTextEnum_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Accept PlainText enum", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .Accept(AcceptTypes.PlainText)
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<AcceptHeaderResponse>();
                Assert.Contains("text/plain", body!.Accept);
            })
            .Run();
    }

    [Test]
    public async Task Accept_HtmlEnum_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Accept Html enum", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .Accept(AcceptTypes.Html)
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<AcceptHeaderResponse>();
                Assert.Contains("text/html", body!.Accept);
            })
            .Run();
    }

    [Test]
    public async Task Accept_AnyEnum_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Accept Any enum", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .Accept(AcceptTypes.Any)
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<AcceptHeaderResponse>();
                Assert.Contains("*/*", body!.Accept);
            })
            .Run();
    }

    [Test]
    public async Task Accept_OctetStreamEnum_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Accept OctetStream enum", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .Accept(AcceptTypes.OctetStream)
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<AcceptHeaderResponse>();
                Assert.Contains("application/octet-stream", body!.Accept);
            })
            .Run();
    }
}
