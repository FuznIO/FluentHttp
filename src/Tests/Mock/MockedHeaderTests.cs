using System.Net;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

/// <summary>
/// Mirrors the live header scenarios by asserting on what the mock captured (no TestApi).
/// </summary>
[TestClass]
public class MockedHeaderTests : Test
{
    [Test]
    public async Task WithHeader_IsSentOnRequest()
    {
        await Scenario()
            .Step("Custom header is sent on the request", async _ =>
            {
                var handler = new FluentHttpMockHandler();
                handler.WhenGet("/api/headers").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/headers").WithHeader("X-Custom", "custom-value").Get();

                var sent = handler.Requests.Single();
                Assert.IsTrue(sent.HasHeader("X-Custom", "custom-value"));
            })
            .Run();
    }

    [Test]
    public async Task WithHeaders_MultipleAreSent()
    {
        await Scenario()
            .Step("Multiple headers are sent on the request", async _ =>
            {
                var handler = new FluentHttpMockHandler();
                handler.WhenGet("/api/headers").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/headers")
                    .WithHeaders(new Dictionary<string, string>
                    {
                        ["X-One"] = "1",
                        ["X-Two"] = "2"
                    })
                    .Get();

                var sent = handler.Requests.Single();
                Assert.IsTrue(sent.HasHeader("X-One", "1"));
                Assert.IsTrue(sent.HasHeader("X-Two", "2"));
            })
            .Run();
    }

    [Test]
    public async Task WithUserAgent_IsSentOnRequest()
    {
        await Scenario()
            .Step("User-Agent header is sent on the request", async _ =>
            {
                var handler = new FluentHttpMockHandler();
                handler.WhenGet("/api/headers").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/headers").WithUserAgent("FluentHttp-Test/1.0").Get();

                var sent = handler.Requests.Single();
                Assert.IsTrue(sent.HasHeader("User-Agent", "FluentHttp-Test/1.0"));
            })
            .Run();
    }
}
