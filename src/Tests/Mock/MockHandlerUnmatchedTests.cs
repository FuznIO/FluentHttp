using System.Net;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

[TestClass]
public class MockHandlerUnmatchedTests : Test
{
    [Test]
    public async Task Unmatched_ThrowsMockHttpException()
    {
        await Scenario()
            .Step("A request that matches no rule throws", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/known").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await Assert.ThrowsAsync<MockHttpException>(
                    () => client.Url("/api/unknown").Get());
            })
            .Run();
    }

    [Test]
    public async Task CatchAllRule_HandlesOtherwiseUnmatchedRequests()
    {
        await Scenario()
            .Step("A trailing WhenAny() rule serves everything not matched earlier", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/known").RespondWith(HttpStatusCode.OK);
                handler.WhenAny().RespondWith(HttpStatusCode.NotFound);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/unknown").Get();

                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            })
            .Run();
    }
}
