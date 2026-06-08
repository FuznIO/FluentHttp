using System.Net;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

/// <summary>
/// Mirrors the live query-parameter scenarios by asserting on what the mock captured (no TestApi).
/// </summary>
[TestClass]
public class MockedQueryParameterTests : Test
{
    [Test]
    public async Task WithQueryParam_IsAppendedToUrl()
    {
        await Scenario()
            .Step("Single query parameter is sent", async _ =>
            {
                var handler = new FluentHttpMockHandler();
                handler.WhenGet("/api/query*").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/query").WithQueryParam("name", "value").Get();

                var sent = handler.Requests.Single();
                Assert.IsTrue(sent.HasQueryParam("name", "value"));
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParam_MultipleAreSent()
    {
        await Scenario()
            .Step("Multiple query parameters are sent", async _ =>
            {
                var handler = new FluentHttpMockHandler();
                handler.WhenGet("/api/query*").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/query")
                    .WithQueryParam("page", "2")
                    .WithQueryParam("size", "50")
                    .Get();

                var sent = handler.Requests.Single();
                Assert.IsTrue(sent.HasQueryParam("page", "2"));
                Assert.IsTrue(sent.HasQueryParam("size", "50"));
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParam_ValueIsUrlEncoded()
    {
        await Scenario()
            .Step("Query parameter values are URL-encoded but decode back to the original", async _ =>
            {
                var handler = new FluentHttpMockHandler();
                handler.WhenGet("/api/query*").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/query").WithQueryParam("q", "a b&c").Get();

                var sent = handler.Requests.Single();
                Assert.IsTrue(sent.HasQueryParam("q", "a b&c"));
            })
            .Run();
    }
}
