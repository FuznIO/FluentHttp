using System.Net;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

/// <summary>
/// Mirrors the live status-code scenarios, served entirely from an in-memory mock (no TestApi).
/// </summary>
[TestClass]
public class MockStatusCodeTests : Test
{
    [Test]
    public async Task Ok_IsSuccessful()
    {
        await Scenario()
            .Step("200 OK is reported as successful", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/status/ok").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/status/ok").Get();

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task NotFound_IsNotSuccessful()
    {
        await Scenario()
            .Step("404 Not Found is reported as not successful", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/status/notfound").RespondWith(HttpStatusCode.NotFound);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/status/notfound").Get();

                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                Assert.IsFalse(response.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task ServerError_EnsureSuccessful_Throws()
    {
        await Scenario()
            .Step("500 causes EnsureSuccessful to throw", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/status/error").RespondWith(HttpStatusCode.InternalServerError);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/status/error").Get();

                Assert.Throws<HttpRequestException>(() => response.EnsureSuccessful());
            })
            .Run();
    }
}
