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
                handler.WhenGet("/api/person/1").RespondWithContent(new PersonDto { Id = 1, Name = "John" });
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
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/person/1").RespondWith(HttpStatusCode.OK);
                handler.WhenAny().RespondWith(HttpStatusCode.NotFound);
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
                var client = handler.CreateClient();

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
    public async Task WhenHead_MatchesHeadRequest()
    {
        await Scenario()
            .Step("WhenHead matches an HTTP HEAD request", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenHead("/api/resource").RespondWith(HttpStatusCode.OK);
                handler.WhenAny().RespondWith(HttpStatusCode.NotFound);
                var client = handler.CreateClient("https://api.example.com/");

                var head = await client.Url("/api/resource").Head();
                var get = await client.Url("/api/resource").Get();

                Assert.AreEqual(HttpStatusCode.OK, head.StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, get.StatusCode); // HEAD rule doesn't match GET
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
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/data").WithHeader("X-Tenant", "acme").RespondWith(HttpStatusCode.OK);
                handler.WhenAny().RespondWith(HttpStatusCode.NotFound);
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
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/search*").WithQueryParam("q", "fluent").RespondWith(HttpStatusCode.OK);
                handler.WhenAny().RespondWith(HttpStatusCode.NotFound);
                var client = handler.CreateClient("https://api.example.com/");

                var matched = await client.Url("/api/search").WithQueryParam("q", "fluent").Get();
                var unmatched = await client.Url("/api/search").WithQueryParam("q", "other").Get();

                Assert.AreEqual(HttpStatusCode.OK, matched.StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, unmatched.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task When_ExactPath_MatchesRequestWithQueryString()
    {
        await Scenario()
            .Step("A pattern without a query matches a request that carries one", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/persons").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/persons?page=2&size=10").Get();

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task When_QueryParamMatcher_WithoutWildcard_Constrains()
    {
        await Scenario()
            .Step("WithQueryParam works on an exact path (no trailing wildcard needed)", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("https://api.example.com/person")
                    .WithQueryParam("id", "2")
                    .RespondWith(HttpStatusCode.OK);
                handler.WhenAny().RespondWith(HttpStatusCode.NotFound);
                var client = handler.CreateClient("https://api.example.com");

                var matched = await client.Url("person?id=2").Get();
                var unmatched = await client.Url("person?id=3").Get();

                Assert.AreEqual(HttpStatusCode.OK, matched.StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, unmatched.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task When_PatternIncludesQuery_MatchesQueryString()
    {
        await Scenario()
            .Step("A pattern that includes a query still matches against the query string", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/persons?page=2").RespondWith(HttpStatusCode.OK);
                handler.WhenAny().RespondWith(HttpStatusCode.NotFound);
                var client = handler.CreateClient("https://api.example.com/");

                var matched = await client.Url("/api/persons?page=2").Get();
                var unmatched = await client.Url("/api/persons?page=3").Get();

                Assert.AreEqual(HttpStatusCode.OK, matched.StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, unmatched.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task When_NoUrl_MatchesAnyUrl()
    {
        await Scenario()
            .Step("WhenAny() and WhenGet() with no URL match any request", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet().RespondWith(HttpStatusCode.OK);
                handler.WhenAny().RespondWith(HttpStatusCode.NotFound);
                var client = handler.CreateClient("https://api.example.com/");

                var a = await client.Url("/anything").Get();
                var b = await client.Url("/else/entirely?x=1").Get();
                var wrongMethod = await client.Url("/anything").Post();

                Assert.AreEqual(HttpStatusCode.OK, a.StatusCode);
                Assert.AreEqual(HttpStatusCode.OK, b.StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, wrongMethod.StatusCode); // WhenGet() still constrains the method
            })
            .Run();
    }

    [Test]
    public async Task WhenAny_NoUrl_MatchesAnyMethodAndUrl()
    {
        await Scenario()
            .Step("WhenAny() matches any method against any URL", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenAny().RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                var get = await client.Url("/a").Get();
                var post = await client.Url("/b?q=1").Post();

                Assert.IsTrue(get.IsSuccessful);
                Assert.IsTrue(post.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task When_PredicateMatchers_Constrain()
    {
        await Scenario()
            .Step("Header, query, and body predicates constrain the match", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/me")
                    .WithHeader("X-Api-Key", v => v.StartsWith("key_"))
                    .WithQueryParam("page", v => int.TryParse(v, out var p) && p > 0)
                    .RespondWith(HttpStatusCode.OK);
                handler.WhenAny().RespondWith(HttpStatusCode.NotFound);
                var http = handler.CreateClient("https://api.example.com/");

                var matched = await http.Url("/api/me?page=3").WithHeader("X-Api-Key", "key_abc").Get();
                var badHeader = await http.Url("/api/me?page=3").WithHeader("X-Api-Key", "nope").Get();
                var badQuery = await http.Url("/api/me?page=0").WithHeader("X-Api-Key", "key_abc").Get();

                Assert.AreEqual(HttpStatusCode.OK, matched.StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, badHeader.StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, badQuery.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task When_LiteralAsteriskInBody_IsNotAWildcard()
    {
        await Scenario()
            .Step("A '*' in the expected body is matched literally, not as a wildcard", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenPost("/api/secret").WithContent("password=***").RespondWith(HttpStatusCode.OK);
                handler.WhenAny().RespondWith(HttpStatusCode.NotFound);
                var http = handler.CreateClient("https://api.example.com/");

                var exact = await http.Url("/api/secret").WithContent("password=***").Post();
                var other = await http.Url("/api/secret").WithContent("password=abc").Post();

                Assert.AreEqual(HttpStatusCode.OK, exact.StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, other.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task WithRequest_CustomPredicate_Constrains()
    {
        await Scenario()
            .Step("WithRequest matches on arbitrary request logic", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenAny()
                    .WithRequest(req => req.Headers.Contains("X-A") && req.RequestUri!.AbsolutePath.StartsWith("/v2/"))
                    .RespondWith(HttpStatusCode.OK);
                handler.WhenAny().RespondWith(HttpStatusCode.NotFound);
                var http = handler.CreateClient("https://api.example.com/");

                var matched = await http.Url("/v2/things").WithHeader("X-A", "1").Get();
                var noHeader = await http.Url("/v2/things").Get();
                var wrongPath = await http.Url("/v1/things").WithHeader("X-A", "1").Get();

                Assert.AreEqual(HttpStatusCode.OK, matched.StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, noHeader.StatusCode);
                Assert.AreEqual(HttpStatusCode.NotFound, wrongPath.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task When_BodyMatcher_Constrains()
    {
        await Scenario()
            .Step("Body object matcher selects the correct rule", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenPost("/api/person")
                    .WithContent(new PersonDto { Name = "Jane" })
                    .RespondWith(HttpStatusCode.Created);
                handler.WhenAny().RespondWith(HttpStatusCode.NotFound);
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
                handler.WhenGet("/api/*").RespondWithContent(new PersonDto { Name = "first" });
                handler.WhenGet("/api/person").RespondWithContent(new PersonDto { Name = "second" });
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/person").Get();

                Assert.AreEqual("first", response.ContentAs<PersonDto>()!.Name);
            })
            .Run();
    }
}
