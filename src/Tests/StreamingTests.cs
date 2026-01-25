namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class StreamingTests : Test
{
    [Test]
    public async Task GetStream_ReturnsStreamResponse()
    {
        await Scenario()
            .Step("Download content as stream", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                await using var streamResponse = await client.Url("/api/stream/download").GetStream();

                Assert.IsTrue(streamResponse.Ok);
                Assert.AreEqual("application/octet-stream", streamResponse.ContentType);
                Assert.AreEqual("test-file.txt", streamResponse.FileName);
                
                var stream = await streamResponse.GetStream();
                Assert.IsNotNull(stream);
                Assert.IsTrue(stream.CanRead);
            })
            .Run();
    }

    [Test]
    public async Task GetStream_GetBytes_ReturnsContent()
    {
        await Scenario()
            .Step("Download content as bytes from stream response", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                await using var streamResponse = await client.Url("/api/stream/download").GetStream();

                Assert.IsTrue(streamResponse.Ok);
                
                var bytes = await streamResponse.GetBytes();
                Assert.IsNotNull(bytes);
                Assert.IsNotEmpty(bytes);
            })
            .Run();
    }

    [Test]
    public async Task GetStream_LargeFile_StreamsCorrectly()
    {
        await Scenario()
            .Step("Stream large file content", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                await using var streamResponse = await client.Url("/api/stream/large").GetStream();

                Assert.IsTrue(streamResponse.Ok);
                
                var stream = await streamResponse.GetStream();
                
                // Read in chunks to verify streaming works
                var buffer = new byte[1024];
                var totalRead = 0L;
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
                {
                    totalRead += bytesRead;
                }
                
                // Should have read 100 KB (100 * 1024 bytes)
                Assert.AreEqual(100 * 1024, totalRead);
            })
            .Run();
    }

    [Test]
    public async Task PostStream_ReturnsStreamResponse()
    {
        await Scenario()
            .Step("POST request with stream response", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                await using var streamResponse = await client.Url("/api/echo")
                    .Body(new { test = "data" })
                    .PostStream();

                Assert.IsTrue(streamResponse.Ok);
                
                var bytes = await streamResponse.GetBytes();
                Assert.IsNotEmpty(bytes);
            })
            .Run();
    }
}
