using System.Net;
using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

[TestClass]
public class MockHandlerMatchingTests : Test
{
    [Test]
    public async Task When_MethodAndRelativeUrl_Matches()
    {
        await Scenario()
            .Step("GET rule matches relative URL", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/person/1").RespondWithJson(new PersonDto { Id = 1, Name = "John" });
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/person/1").Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.AreEqual("John", response.ContentAs<PersonDto>()!.Name);
            })
            .Run();
    }

    [Test]
    public async Task When_WrongMethod_DoesNotMatch()
    {
        await Scenario()
            .Step("POST request does not match GET rule", async _ =>
            {
                var handler = new MockHttpHandler().WithFallback(MockFallbackBehavior.RespondNotFound);
                handler.WhenGet("/api/person/1").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/person/1").Post();

                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task When_WildcardPattern_Matches()
    {
        await Scenario()
            .Step("Wildcard pattern matches any path segment", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/person/*").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/person/42").Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task When_AbsolutePattern_Matches()
    {
        await Scenario()
            .Step("Absolute URL pattern matches", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("https://api.example.com/api/person/1").RespondWith(HttpStatusCode.OK);
                var client = handler.ToHttpClient();

                var response = await client.Url("https://api.example.com/api/person/1").Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task When_WildcardHostPattern_Matches()
    {
        await Scenario()
            .Step("Absolute pattern with a wildcard host matches the request URL", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("https://*/api/person/1").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/person/1").Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task WhenAny_MatchesAnyMethod()
    {
        await Scenario()
            .Step("WhenAny matches both GET and DELETE", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenAny("/api/resource").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                var get = await client.Url("/api/resource").Get();
                var delete = await client.Url("/api/resource").Delete();

                Assert.IsTrue(get.IsSuccessful);
                Assert.IsTrue(delete.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task When_HeaderMatcher_Constrains()
    {
        await Scenario()
            .Step("Header matcher selects the correct rule", async _ =>
            {
                var handler = new MockHttpHandler().WithFallback(MockFallbackBehavior.RespondNotFound);
                handler.WhenGet("/api/data").WithHeader("X-Tenant", "acme").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                var matched = await client.Url("/api/data").WithHeader("X-Tenant", "acme").Get();
                var unmatched = await client.Url("/api/data").WithHeader("X-Tenant", "other").Get();

                Assert.AreEqual(HttpStatusCode.OK, matched.StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, unmatched.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task When_QueryParamMatcher_Constrains()
    {
        await Scenario()
            .Step("Query param matcher selects the correct rule", async _ =>
            {
                var handler = new MockHttpHandler().WithFallback(MockFallbackBehavior.RespondNotFound);
                handler.WhenGet("/api/search*").WithQueryParam("q", "fluent").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                var matched = await client.Url("/api/search").WithQueryParam("q", "fluent").Get();
                var unmatched = await client.Url("/api/search").WithQueryParam("q", "other").Get();

                Assert.AreEqual(HttpStatusCode.OK, matched.StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, unmatched.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task When_BodyMatcher_Constrains()
    {
        await Scenario()
            .Step("Body object matcher selects the correct rule", async _ =>
            {
                var handler = new MockHttpHandler().WithFallback(MockFallbackBehavior.RespondNotFound);
                handler.WhenPost("/api/person")
                    .WithContent(new PersonDto { Name = "Jane" })
                    .RespondWith(HttpStatusCode.Created);
                var client = handler.CreateClient("https://api.example.com/");

                var matched = await client.Url("/api/person").WithContent(new PersonDto { Name = "Jane" }).Post();
                var unmatched = await client.Url("/api/person").WithContent(new PersonDto { Name = "Bob" }).Post();

                Assert.AreEqual(HttpStatusCode.Created, matched.StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, unmatched.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task When_MultipleRules_FirstMatchWins()
    {
        await Scenario()
            .Step("Rules are evaluated in registration order", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/*").RespondWith(HttpStatusCode.OK, new PersonDto { Name = "first" });
                handler.WhenGet("/api/person").RespondWith(HttpStatusCode.OK, new PersonDto { Name = "second" });
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/person").Get();

                Assert.AreEqual("first", response.ContentAs<PersonDto>()!.Name);
            })
            .Run();
    }
}
