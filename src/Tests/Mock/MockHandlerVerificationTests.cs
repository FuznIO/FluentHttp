using System.Net;
using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

[TestClass]
public class MockHandlerVerificationTests : Test
{
    [Test]
    public async Task VerifyMatched_PassesForExpectedCount()
    {
        await Scenario()
            .Step("VerifyMatched succeeds when count matches", async _ =>
            {
                var handler = new FluentHttpMockHandler();
                var stub = handler.WhenGet("/api/ping");
                stub.RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/ping").Get();
                await client.Url("/api/ping").Get();

                Assert.AreEqual(2, stub.MatchCount);
                handler.VerifyMatched(stub, 2);
            })
            .Run();
    }

    [Test]
    public async Task VerifyMatched_ThrowsForWrongCount()
    {
        await Scenario()
            .Step("VerifyMatched throws when count differs", async _ =>
            {
                var handler = new FluentHttpMockHandler();
                var stub = handler.WhenGet("/api/ping");
                stub.RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/ping").Get();

                Assert.Throws<FluentHttpMockException>(() => handler.VerifyMatched(stub, 2));
            })
            .Run();
    }

    [Test]
    public async Task Requests_CapturesMethodUrlHeadersAndBody()
    {
        await Scenario()
            .Step("Captured request exposes what FluentHttp sent", async _ =>
            {
                var handler = new FluentHttpMockHandler();
                handler.WhenPost("/api/person*").RespondWith(HttpStatusCode.Created);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/person")
                    .WithQueryParam("notify", "true")
                    .WithAuthBearer("token-123")
                    .WithContent(new PersonDto { Name = "Jane" })
                    .Post();

                var sent = handler.Requests.Single();
                Assert.AreEqual(HttpMethod.Post, sent.Method);
                Assert.IsTrue(sent.HasHeader("Authorization", "Bearer token-123"));
                Assert.IsTrue(sent.HasQueryParam("notify", "true"));
                Assert.AreEqual("Jane", sent.ContentAs<PersonDto>()!.Name);
            })
            .Run();
    }

    [Test]
    public async Task VerifyNoUnmatched_ThrowsWhenARequestMissed()
    {
        await Scenario()
            .Step("VerifyNoUnmatched flags missed requests", async _ =>
            {
                var handler = new FluentHttpMockHandler().WithFallback(MockFallbackBehavior.RespondNotFound);
                handler.WhenGet("/api/known").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/known").Get();
                await client.Url("/api/unknown").Get();

                Assert.Throws<FluentHttpMockException>(() => handler.VerifyNoUnmatched());
            })
            .Run();
    }

    [Test]
    public async Task Reset_ClearsCapturesAndCounts()
    {
        await Scenario()
            .Step("Reset clears captures and match counts but keeps stubs", async _ =>
            {
                var handler = new FluentHttpMockHandler();
                var stub = handler.WhenGet("/api/ping");
                stub.RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/ping").Get();
                handler.Reset();

                Assert.AreEqual(0, stub.MatchCount);
                Assert.IsEmpty(handler.Requests);

                // Stub still works after reset.
                var response = await client.Url("/api/ping").Get();
                Assert.IsTrue(response.IsSuccessful);
                Assert.AreEqual(1, stub.MatchCount);
            })
            .Run();
    }
}
