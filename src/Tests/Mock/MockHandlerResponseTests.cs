using System.Net;
using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

[TestClass]
public class MockHandlerResponseTests : Test
{
    [Test]
    public async Task RespondWithContent_SerializesObjectAsJson()
    {
        await Scenario()
            .Step("JSON response is deserializable and typed", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/person/1")
                    .RespondWithContent(new PersonDto { Id = 1, Name = "John", Email = "john@x.com", Age = 30 });
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/person/1").Get();

                Assert.AreEqual("application/json", response.ContentType);
                var person = response.ContentAs<PersonDto>()!;
                Assert.AreEqual(1, person.Id);
                Assert.AreEqual("John", person.Name);
                Assert.AreEqual(30, person.Age);
            })
            .Run();
    }

    [Test]
    public async Task RespondWith_StatusOnly_ReturnsStatusWithEmptyBody()
    {
        await Scenario()
            .Step("Status-only response", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenDelete("/api/person/1").RespondWith(HttpStatusCode.NoContent);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/person/1").Delete();

                Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
                Assert.AreEqual(string.Empty, response.Content);
            })
            .Run();
    }

    [Test]
    public async Task RespondWith_StatusAndJsonBody_ReturnsBoth()
    {
        await Scenario()
            .Step("Status plus JSON body overload", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenPost("/api/person")
                    .RespondWithContent(new PersonDto { Id = 7, Name = "New" }, statusCode: HttpStatusCode.Created);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/person").WithContent(new PersonDto { Name = "New" }).Post();

                Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
                Assert.AreEqual(7, response.ContentAs<PersonDto>()!.Id);
            })
            .Run();
    }

    [Test]
    public async Task RespondWithContent_ReturnsRawStringAndContentType()
    {
        await Scenario()
            .Step("Raw string content response", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/text").RespondWithContent("hello world", "text/plain");
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/text").Get();

                Assert.AreEqual("hello world", response.Content);
                Assert.AreEqual("text/plain", response.ContentType);
            })
            .Run();
    }

    [Test]
    public async Task WithResponseHeader_IsReturnedOnResponse()
    {
        await Scenario()
            .Step("Custom response header is present", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/data")
                    .WithResponseHeader("X-Custom", "abc")
                    .RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/data").Get();

                Assert.IsTrue(response.Headers.TryGetValues("X-Custom", out var values));
                Assert.AreEqual("abc", values!.Single());
            })
            .Run();
    }

    [Test]
    public async Task RespondWith_Factory_BuildsResponseFromRequest()
    {
        await Scenario()
            .Step("Response factory echoes request info", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/echo*").RespondWith(request =>
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(request.RequestUri!.Query)
                    });
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/echo").WithQueryParam("x", "1").Get();

                Assert.AreEqual("?x=1", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task RespondWith_CustomResponseMessage_IsReturned()
    {
        await Scenario()
            .Step("Fully custom HttpResponseMessage is returned", async _ =>
            {
                var custom = new HttpResponseMessage(HttpStatusCode.Accepted)
                {
                    Content = new StringContent("queued")
                };
                var handler = new MockHttpHandler();
                handler.WhenPost("/api/jobs").RespondWith(custom);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/jobs").WithContent(new { id = 1 }).Post();

                Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
                Assert.AreEqual("queued", response.Content);
            })
            .Run();
    }
}
