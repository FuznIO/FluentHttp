using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class HttpStreamResponsePropertiesTests : Test
{
    [Test]
    public async Task StreamResponse_ContentLength_IsAccessible()
    {
        await Scenario()
            .Step("Stream response content length is accessible", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                await using var streamResponse = await client.Url("/api/stream/download").GetStream();

                Assert.IsTrue(streamResponse.IsSuccessful);
                // ContentLength may or may not be set depending on server
                Assert.IsNotNull(streamResponse.ContentLength);
            })
            .Run();
    }

    [Test]
    public async Task StreamResponse_ContentType_IsAccessible()
    {
        await Scenario()
            .Step("Stream response content type is accessible", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                await using var streamResponse = await client.Url("/api/stream/download").GetStream();

                Assert.IsTrue(streamResponse.IsSuccessful);
                Assert.IsNotNull(streamResponse.ContentType);
            })
            .Run();
    }

    [Test]
    public async Task StreamResponse_FileName_IsAccessible()
    {
        await Scenario()
            .Step("Stream response filename is accessible", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                await using var streamResponse = await client.Url("/api/stream/download").GetStream();

                Assert.IsTrue(streamResponse.IsSuccessful);
                Assert.IsNotNull(streamResponse.FileName);
            })
            .Run();
    }

    [Test]
    public async Task StreamResponse_Headers_AreAccessible()
    {
        await Scenario()
            .Step("Stream response headers are accessible", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                await using var streamResponse = await client.Url("/api/stream/download").GetStream();

                Assert.IsTrue(streamResponse.IsSuccessful);
                Assert.IsNotNull(streamResponse.Headers);
            })
            .Run();
    }

    [Test]
    public async Task StreamResponse_ContentHeaders_AreAccessible()
    {
        await Scenario()
            .Step("Stream response content headers are accessible", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                await using var streamResponse = await client.Url("/api/stream/download").GetStream();

                Assert.IsTrue(streamResponse.IsSuccessful);
                Assert.IsNotNull(streamResponse.ContentHeaders);
            })
            .Run();
    }

    [Test]
    public async Task StreamResponse_InnerResponse_IsAccessible()
    {
        await Scenario()
            .Step("Stream response inner response is accessible", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                await using var streamResponse = await client.Url("/api/stream/download").GetStream();

                Assert.IsTrue(streamResponse.IsSuccessful);
                Assert.IsNotNull(streamResponse.InnerResponse);
            })
            .Run();
    }

    [Test]
    public async Task StreamResponse_StatusCode_IsAccessible()
    {
        await Scenario()
            .Step("Stream response status code is accessible", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                await using var streamResponse = await client.Url("/api/stream/download").GetStream();

                Assert.AreEqual(System.Net.HttpStatusCode.OK, streamResponse.StatusCode);
            })
            .Run();
    }
}
