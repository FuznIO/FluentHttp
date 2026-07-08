using System.Net;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

/// <summary>
/// Mirrors the live query-parameter scenarios by asserting on what the mock captured (no TestApi).
/// </summary>
[TestClass]
public class MockQueryParameterTests : Test
{
    [Test]
    public async Task WithQueryParam_IsAppendedToUrl()
    {
        await Scenario()
            .Step("Single query parameter is sent", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/query*").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/query").WithQueryParam("name", "value").Get();

                var sent = handler.Requests.Single();
                Assert.AreEqual("value", sent.Query["name"].Single());
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParam_MultipleAreSent()
    {
        await Scenario()
            .Step("Multiple query parameters are sent", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/query*").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/query")
                    .WithQueryParam("page", "2")
                    .WithQueryParam("size", "50")
                    .Get();

                var sent = handler.Requests.Single();
                Assert.AreEqual("2", sent.Query["page"].Single());
                Assert.AreEqual("50", sent.Query["size"].Single());
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParam_ValueIsUrlEncoded()
    {
        await Scenario()
            .Step("Query parameter values are URL-encoded but decode back to the original", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/query*").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/query").WithQueryParam("q", "a b&c").Get();

                var sent = handler.Requests.Single();
                Assert.AreEqual("a b&c", sent.Query["q"].Single());
            })
            .Run();
    }

    [Test]
    public async Task Query_KeepsAllValuesOfARepeatedParameter()
    {
        await Scenario()
            .Step("A repeated query parameter keeps every value", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/search*").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/search")
                    .WithQueryParam("tag", "a")
                    .WithQueryParam("tag", "b")
                    .Get();

                var sent = handler.Requests.Single();
                CollectionAssert.AreEquivalent(new[] { "a", "b" }, sent.Query["tag"]);
            })
            .Run();
    }
}
