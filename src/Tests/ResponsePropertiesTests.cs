namespace Fuzn.FluentHttp.Tests;
using Fuzn.TestFuzn;

[TestClass]
public class ResponsePropertiesTests : Test
{
    [Test]
    public async Task Response_Headers_AreAccessible()
    {
        await Scenario()
            .Step("Access response headers", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/response/json").Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.IsNotNull(response.Headers);
            })
            .Run();
    }

    [Test]
    public async Task Response_ContentHeaders_AreAccessible()
    {
        await Scenario()
            .Step("Access content headers", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/response/json").Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.IsNotNull(response.ContentHeaders);
                Assert.IsNotNull(response.ContentHeaders.ContentType);
            })
            .Run();
    }

    [Test]
    public async Task Response_InnerResponse_IsAccessible()
    {
        await Scenario()
            .Step("Access inner HttpResponseMessage", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/response/json").Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.IsNotNull(response.InnerResponse);
                Assert.IsTrue(response.InnerResponse.IsSuccessStatusCode);
            })
            .Run();
    }

    [Test]
    public async Task Response_RawResponse_ContainsResponseInfo()
    {
        await Scenario()
            .Step("Access raw response string", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/response/json").Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.IsNotNull(response.RawResponse);
                Assert.IsGreaterThan(0, response.RawResponse.Length);
            })
            .Run();
    }
}
