using System.Net;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

[TestClass]
public class MockHandlerFallbackTests : Test
{
    [Test]
    public async Task DefaultFallback_Throw_ThrowsOnUnmatchedRequest()
    {
        await Scenario()
            .Step("Unmatched request throws by default", async _ =>
            {
                var handler = new FluentHttpMockHandler();
                handler.WhenGet("/api/known").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await Assert.ThrowsAsync<FluentHttpMockException>(
                    () => client.Url("/api/unknown").Get());
            })
            .Run();
    }

    [Test]
    public async Task Fallback_RespondNotFound_Returns404()
    {
        await Scenario()
            .Step("Unmatched request returns 404 when configured", async _ =>
            {
                var handler = new FluentHttpMockHandler().WithFallback(MockFallbackBehavior.RespondNotFound);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/unknown").Get();

                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task UnmatchedCount_TracksMissedRequests()
    {
        await Scenario()
            .Step("UnmatchedCount increments per miss", async _ =>
            {
                var handler = new FluentHttpMockHandler().WithFallback(MockFallbackBehavior.RespondNotFound);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/a").Get();
                await client.Url("/api/b").Get();

                Assert.AreEqual(2, handler.UnmatchedCount);
            })
            .Run();
    }
}
