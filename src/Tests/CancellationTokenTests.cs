using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class CancellationTokenTests : Test
{
    [Test]
    public async Task Get_WithCancellation_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Cancelled GET request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/delay/1000").Get(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task Post_WithCancellation_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Cancelled POST request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/echo")
                        .Body(new { test = "data" })
                        .Post(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task Put_WithCancellation_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Cancelled PUT request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/echo")
                        .Body(new { id = 1 })
                        .Put(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task Delete_WithCancellation_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Cancelled DELETE request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/echo").Delete(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task Patch_WithCancellation_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Cancelled PATCH request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/echo")
                        .Body(new { field = "value" })
                        .Patch(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }
}
