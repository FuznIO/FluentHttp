using System.Net;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

[TestClass]
public class MockHandlerEnhancementsTests : Test
{
    [Test]
    public async Task CustomResponse_CanBeMatchedMultipleTimes()
    {
        await Scenario()
            .Step("A custom HttpResponseMessage is reusable across repeated matches", async _ =>
            {
                var custom = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("queued")
                };
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/jobs").RespondWith(custom);
                var client = handler.CreateClient("https://api.example.com/");

                var first = await client.Url("/api/jobs").Get();
                var second = await client.Url("/api/jobs").Get();

                // Without per-match cloning the second read would see a disposed/consumed response.
                Assert.AreEqual("queued", first.Content);
                Assert.AreEqual("queued", second.Content);
            })
            .Run();
    }

    [Test]
    public async Task WithResponseHeader_AppliesToCustomAndFactoryResponses()
    {
        await Scenario()
            .Step("Stub response headers apply to custom and factory responses too", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/custom")
                    .WithResponseHeader("X-Source", "mock")
                    .RespondWith(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("c") });
                handler.WhenGet("/api/factory")
                    .WithResponseHeader("X-Source", "mock")
                    .RespondWith(_ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("f") });
                var client = handler.CreateClient("https://api.example.com/");

                var custom = await client.Url("/api/custom").Get();
                var factory = await client.Url("/api/factory").Get();

                Assert.IsTrue(custom.Headers.TryGetValues("X-Source", out var cv) && cv!.Single() == "mock");
                Assert.IsTrue(factory.Headers.TryGetValues("X-Source", out var fv) && fv!.Single() == "mock");
            })
            .Run();
    }

    [Test]
    public async Task RespondWith_AsyncFactory_IsAwaited()
    {
        await Scenario()
            .Step("Asynchronous response factory builds the response", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/async").RespondWith(async (request, _) =>
                {
                    await Task.Yield();
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(request.RequestUri!.AbsolutePath)
                    };
                });
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/async").Get();

                Assert.AreEqual("/api/async", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task Requests_CanBeAssertedOverWithLinq()
    {
        await Scenario()
            .Step("Assert over handler.Requests with the test framework", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenAny("/api/*").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/a").WithAuthBearer("t1").Get();
                await client.Url("/api/b").Post();

                Assert.HasCount(2, handler.Requests);
                Assert.IsTrue(handler.Requests.Any(r => r.Method == HttpMethod.Get));
                Assert.AreEqual(1, handler.Requests.Count(r => r.Headers.TryGetValue("Authorization", out var a) && a.Contains("Bearer t1")));
                Assert.IsFalse(handler.Requests.Any(r => r.Method == HttpMethod.Delete));
            })
            .Run();
    }

    [Test]
    public async Task CapturedRequest_ExposesRawBodyBytes()
    {
        await Scenario()
            .Step("Binary request bodies are captured as raw bytes", async _ =>
            {
                var payload = new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE };
                var handler = new MockHttpHandler();
                handler.WhenPost("/api/upload").RespondWith(HttpStatusCode.Created);
                using var client = handler.CreateClient("https://api.example.com/");

                using var request = new HttpRequestMessage(HttpMethod.Post, "api/upload")
                {
                    Content = new ByteArrayContent(payload)
                };
                using var response = await client.SendAsync(request);

                var sent = handler.Requests.Single();
                Assert.IsNotNull(sent.ContentBytes);
                Assert.IsTrue(payload.SequenceEqual(sent.ContentBytes!));
            })
            .Run();
    }
}
