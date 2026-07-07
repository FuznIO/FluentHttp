using System.Net;
using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

[TestClass]
public class MockHandlerSequenceTests : Test
{
    [Test]
    public async Task ThenRespondWith_AdvancesThroughSequence()
    {
        await Scenario()
            .Step("Each call returns the next response in the sequence", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/job")
                    .RespondWith(HttpStatusCode.Accepted)
                    .ThenRespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                var first = await client.Url("/api/job").Get();
                var second = await client.Url("/api/job").Get();

                Assert.AreEqual(HttpStatusCode.Accepted, first.StatusCode);
                Assert.AreEqual(HttpStatusCode.OK, second.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task ThenRespondWith_LastResponseRepeatsOnceExhausted()
    {
        await Scenario()
            .Step("After the sequence is exhausted, the last response repeats", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/job")
                    .RespondWith(HttpStatusCode.Accepted)
                    .ThenRespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/job").Get(); // Accepted
                var second = await client.Url("/api/job").Get(); // OK
                var third = await client.Url("/api/job").Get(); // OK (repeats)

                Assert.AreEqual(HttpStatusCode.OK, second.StatusCode);
                Assert.AreEqual(HttpStatusCode.OK, third.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task ThenRespondWithContent_ReturnsDifferentBodiesPerCall()
    {
        await Scenario()
            .Step("Polling returns pending then done", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/job/status")
                    .RespondWithContent(new PersonDto { Name = "pending" })
                    .ThenRespondWithContent(new PersonDto { Name = "done" });
                var client = handler.CreateClient("https://api.example.com/");

                var first = await client.Url("/api/job/status").Get<PersonDto>();
                var second = await client.Url("/api/job/status").Get<PersonDto>();

                Assert.AreEqual("pending", first.Data!.Name);
                Assert.AreEqual("done", second.Data!.Name);
            })
            .Run();
    }

    [Test]
    public async Task ThenRespondWith_FailThenSucceed_SupportsRetryTesting()
    {
        await Scenario()
            .Step("First call fails, retry succeeds", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/flaky")
                    .RespondWithException(new HttpRequestException("transient"))
                    .ThenRespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                // First attempt throws.
                await Assert.ThrowsAsync<HttpRequestException>(() => client.Url("/api/flaky").Get());

                // Manual "retry" succeeds.
                var retry = await client.Url("/api/flaky").Get();
                Assert.AreEqual(HttpStatusCode.OK, retry.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task RespondWith_RestartsSequence()
    {
        await Scenario()
            .Step("A later RespondWith replaces the existing sequence", async _ =>
            {
                var handler = new MockHttpHandler();
                var rule = handler.WhenGet("/api/job");
                rule.RespondWith(HttpStatusCode.Accepted).ThenRespondWith(HttpStatusCode.OK);

                // Reconfigure: replace the whole sequence with a single response.
                rule.RespondWith(HttpStatusCode.InternalServerError);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/job").Get();

                Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task ThenRespondWith_BeforeRespondWith_Throws()
    {
        await Scenario()
            .Step("ThenRespondWith without a preceding RespondWith throws", async _ =>
            {
                var handler = new MockHttpHandler();
                var rule = handler.WhenGet("/api/job");

                Assert.Throws<InvalidOperationException>(() => rule.ThenRespondWith(HttpStatusCode.OK));
            })
            .Run();
    }
}
