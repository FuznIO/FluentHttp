using System.Net;
using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

[TestClass]
public class MockHandlerVerificationTests : Test
{
    [Test]
    public async Task MatchCount_ReflectsNumberOfMatches()
    {
        await Scenario()
            .Step("A rule's MatchCount tracks how many requests it handled", async _ =>
            {
                var handler = new MockHttpHandler();
                var rule = handler.WhenGet("/api/ping");
                rule.RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/ping").Get();
                await client.Url("/api/ping").Get();

                Assert.AreEqual(2, rule.MatchCount);
            })
            .Run();
    }

    [Test]
    public async Task Requests_CapturesMethodUrlHeadersAndBody()
    {
        await Scenario()
            .Step("Captured request exposes what FluentHttp sent", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenPost("/api/person*").RespondWith(HttpStatusCode.Created);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/person")
                    .WithQueryParam("notify", "true")
                    .WithAuthBearer("token-123")
                    .WithContent(new PersonDto { Name = "Jane" })
                    .Post();

                var sent = handler.Requests.Single();
                Assert.AreEqual(HttpMethod.Post, sent.Method);
                Assert.IsTrue(sent.Headers["Authorization"].Contains("Bearer token-123"));
                Assert.AreEqual("true", sent.Query["notify"].Single());
                Assert.AreEqual("Jane", sent.ContentAs<PersonDto>()!.Name);
            })
            .Run();
    }
}
