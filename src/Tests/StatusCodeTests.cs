namespace Fuzn.FluentHttp.Tests;
using Fuzn.TestFuzn;

[TestClass]
public class StatusCodeTests : Test
{
    [Test]
    public async Task StatusCode_IsSuccessful_ReturnsTrue()
    {
        await Scenario()
            .Step("Verify IsSuccessful property for 200 response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/status/ok").Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task StatusCode_Created_ReturnsCorrectly()
    {
        await Scenario()
            .Step("Verify 201 Created response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/status/created").Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task StatusCode_NoContent_ReturnsCorrectly()
    {
        await Scenario()
            .Step("Verify 204 No Content response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/status/nocontent").Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
                Assert.AreEqual(string.Empty, response.Body);
            })
            .Run();
    }

    [Test]
    public async Task StatusCode_BadRequest_ReturnsCorrectly()
    {
        await Scenario()
            .Step("Verify 400 Bad Request response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/status/badrequest").Get();

                Assert.IsFalse(response.IsSuccessful);
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task StatusCode_NotFound_ReturnsCorrectly()
    {
        await Scenario()
            .Step("Verify 404 Not Found response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/status/notfound").Get();

                Assert.IsFalse(response.IsSuccessful);
                Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task StatusCode_InternalServerError_ReturnsCorrectly()
    {
        await Scenario()
            .Step("Verify 500 Internal Server Error response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/status/error").Get();

                Assert.IsFalse(response.IsSuccessful);
                Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task StatusCode_Custom_ReturnsCorrectly()
    {
        await Scenario()
            .Step("Verify custom status code response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/status/418").Get();

                Assert.AreEqual((System.Net.HttpStatusCode)418, response.StatusCode);
            })
            .Run();
    }
}
