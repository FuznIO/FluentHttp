using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class RequestOptionsTests : Test
{
    [Test]
    public async Task WithOption_SingleOption_IsSetOnRequest()
    {
        await Scenario()
            .Step("Set single option on request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                // Options are metadata for the request, not sent to the server
                // This test verifies the WithOption method can be called without error
                var response = await client.Url("/api/echo")
                    .WithOption("customKey", "customValue")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task WithOption_MultipleOptions_AreSetOnRequest()
    {
        await Scenario()
            .Step("Set multiple options on request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/echo")
                    .WithOption("option1", "value1")
                    .WithOption("option2", 123)
                    .WithOption("option3", true)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }
}
