using System.Net;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

[TestClass]
public class MockHandlerFailureTests : Test
{
    [Test]
    public async Task RespondWithException_ThrowsConfiguredException()
    {
        await Scenario()
            .Step("Configured transport exception propagates", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/down").RespondWithException(new HttpRequestException("connection refused"));
                var client = handler.CreateClient("https://api.example.com/");

                var ex = await Assert.ThrowsAsync<HttpRequestException>(
                    () => client.Url("/api/down").Get());

                Assert.AreEqual("connection refused", ex.Message);
            })
            .Run();
    }

    [Test]
    public async Task RespondWithTimeout_ThrowsTaskCanceled()
    {
        await Scenario()
            .Step("Configured timeout throws TaskCanceledException", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/slow").RespondWithTimeout();
                var client = handler.CreateClient("https://api.example.com/");

                await Assert.ThrowsAsync<TaskCanceledException>(
                    () => client.Url("/api/slow").Get());
            })
            .Run();
    }

    [Test]
    public async Task WithResponseDelay_AndClientTimeout_CancelsRequest()
    {
        await Scenario()
            .Step("Delay longer than the client timeout cancels the request", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/slow")
                    .WithResponseDelay(TimeSpan.FromSeconds(5))
                    .RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await Assert.ThrowsAsync<TaskCanceledException>(
                    () => client.Url("/api/slow").WithTimeout(TimeSpan.FromMilliseconds(50)).Get());
            })
            .Run();
    }
}
