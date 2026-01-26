using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class TimeoutTests : Test
{
    [Test]
    public async Task Timeout_CompletesWithinTimeout_ReturnsSuccess()
    {
        await Scenario()
            .Step("Request completes before timeout", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/delay/100")
                    .Timeout(TimeSpan.FromSeconds(5))
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task Timeout_ExceedsTimeout_ThrowsTaskCanceledException()
    {
        await Scenario()
            .Step("Request times out", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/delay/5000")
                        .Timeout(TimeSpan.FromMilliseconds(100))
                        .Get();
                }
                catch (TaskCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected TaskCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task Timeout_StreamRequest_CompletesWithinTimeout()
    {
        await Scenario()
            .Step("Stream request completes before timeout", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                await using var streamResponse = await client.Url("/api/stream/download")
                    .Timeout(TimeSpan.FromSeconds(5))
                    .GetStream();

                Assert.IsTrue(streamResponse.IsSuccessful);
            })
            .Run();
    }
}
