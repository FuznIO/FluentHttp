using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class HttpMethodTests : Test
{
    [Test]
    public async Task Get_ReturnsSuccess()
    {
        await Scenario()
            .Step("Send GET request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/methods/get").Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
                
                var body = response.ContentAs<MethodResponse>();
                Assert.AreEqual("GET", body!.Method);
                Assert.IsTrue(body.Success);
            })
            .Run();
    }

    [Test]
    public async Task Post_WithBody_ReturnsSuccess()
    {
        await Scenario()
            .Step("Send POST request with body", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var payload = new { name = "Test", value = 123 };
                var response = await client.Url("/api/methods/post")
                    .WithContent(payload)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<MethodResponse>();
                Assert.AreEqual("POST", body!.Method);
                Assert.IsTrue(body.Success);
            })
            .Run();
    }

    [Test]
    public async Task Put_WithBody_ReturnsSuccess()
    {
        await Scenario()
            .Step("Send PUT request with body", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var payload = new { id = 1, name = "Updated" };
                var response = await client.Url("/api/methods/put")
                    .WithContent(payload)
                    .Put();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<MethodResponse>();
                Assert.AreEqual("PUT", body!.Method);
            })
            .Run();
    }

    [Test]
    public async Task Delete_ReturnsSuccess()
    {
        await Scenario()
            .Step("Send DELETE request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/methods/delete").Delete();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<MethodResponse>();
                Assert.AreEqual("DELETE", body!.Method);
            })
            .Run();
    }

    [Test]
    public async Task Patch_WithBody_ReturnsSuccess()
    {
        await Scenario()
            .Step("Send PATCH request with body", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var payload = new { field = "updated" };
                var response = await client.Url("/api/methods/patch")
                    .WithContent(payload)
                    .Patch();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<MethodResponse>();
                Assert.AreEqual("PATCH", body!.Method);
            })
            .Run();
    }

    [Test]
    public async Task Head_ReturnsSuccess()
    {
        await Scenario()
            .Step("Send HEAD request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/methods/head").Head();

                Assert.IsTrue(response.IsSuccessful);
                Assert.AreEqual(string.Empty, response.Content);
            })
            .Run();
    }

    [Test]
    public async Task Options_ReturnsSuccess()
    {
        await Scenario()
            .Step("Send OPTIONS request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/methods/options").Options();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }
}
