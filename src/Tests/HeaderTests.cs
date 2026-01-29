using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class HeaderTests : Test
{
    [Test]
    public async Task WithHeader_SingleHeader_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with single custom header", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/headers/custom")
                    .WithHeader("X-Custom-Header", "CustomValue123")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<CustomHeaderResponse>();
                Assert.AreEqual("CustomValue123", body!.CustomHeader);
            })
            .Run();
    }

    [Test]
    public async Task WithHeaders_MultipleHeaders_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with multiple headers", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var headers = new Dictionary<string, string>
                {
                    ["X-Custom-Header"] = "Value1",
                    ["X-Another-Header"] = "Value2"
                };

                var response = await client.Url("/api/headers/echo")
                    .WithHeaders(headers)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<HeadersEchoResponse>();
                Assert.AreEqual("Value1", body!.Headers["X-Custom-Header"]);
                Assert.AreEqual("Value2", body!.Headers["X-Another-Header"]);
            })
            .Run();
    }

    [Test]
    public async Task WithUserAgent_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with custom User-Agent", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/headers/custom")
                    .WithUserAgent("FluentHttp-Test/1.0")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<CustomHeaderResponse>();
                Assert.AreEqual("FluentHttp-Test/1.0", body!.UserAgent);
            })
            .Run();
    }

    [Test]
    public async Task WithAccept_JsonEnum_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Accept header from enum", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .WithAccept(AcceptTypes.Json)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<AcceptHeaderResponse>();
                Assert.Contains("application/json", body!.Accept);
            })
            .Run();
    }

    [Test]
    public async Task WithAccept_CustomString_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with custom Accept header string", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/headers/accept")
                    .WithAccept("application/pdf")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<AcceptHeaderResponse>();
                Assert.Contains("application/pdf", body!.Accept);
            })
            .Run();
    }
}
