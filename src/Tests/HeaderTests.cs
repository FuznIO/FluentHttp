using Fuzn.FluentHttp.TestApi.Models;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class HeaderTests : Test
{
    [Test]
    public async Task Header_SingleHeader_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with single custom header", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/headers/custom")
                    .Header("X-Custom-Header", "CustomValue123")
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<CustomHeaderResponse>();
                Assert.AreEqual("CustomValue123", body!.CustomHeader);
            })
            .Run();
    }

    [Test]
    public async Task Headers_MultiplHeaders_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with multiple headers", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var headers = new Dictionary<string, string>
                {
                    ["X-Custom-Header"] = "Value1",
                    ["X-Another-Header"] = "Value2"
                };

                var response = await client.Url("/api/headers/echo")
                    .Headers(headers)
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<HeadersEchoResponse>();
                Assert.AreEqual("Value1", body!.Headers["X-Custom-Header"]);
                Assert.AreEqual("Value2", body!.Headers["X-Another-Header"]);
            })
            .Run();
    }

    [Test]
    public async Task UserAgent_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with custom User-Agent", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/headers/custom")
                    .UserAgent("FluentHttp-Test/1.0")
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<CustomHeaderResponse>();
                Assert.AreEqual("FluentHttp-Test/1.0", body!.UserAgent);
            })
            .Run();
    }

    [Test]
    public async Task Accept_JsonEnum_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Accept header from enum", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .Accept(AcceptTypes.Json)
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<AcceptHeaderResponse>();
                Assert.Contains("application/json", body!.Accept);
            })
            .Run();
    }

    [Test]
    public async Task Accept_CustomString_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with custom Accept header string", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .Accept("application/pdf")
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<AcceptHeaderResponse>();
                Assert.Contains("application/pdf", body!.Accept);
            })
            .Run();
    }
}
