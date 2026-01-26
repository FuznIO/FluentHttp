using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class RequestOptionsTests : Test
{
    [Test]
    public async Task Options_SingleOption_IsSetOnRequest()
    {
        await Scenario()
            .Step("Set single option on request", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                // Options are metadata for the request, not sent to the server
                // This test verifies the Options method can be called without error
                var response = await client.Url("/api/echo")
                    .Options("customKey", "customValue")
                    .Get();

                Assert.IsTrue(response.Ok);
            })
            .Run();
    }

    [Test]
    public async Task Options_MultipleOptions_AreSetOnRequest()
    {
        await Scenario()
            .Step("Set multiple options on request", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/echo")
                    .Options("option1", "value1")
                    .Options("option2", 123)
                    .Options("option3", true)
                    .Get();

                Assert.IsTrue(response.Ok);
            })
            .Run();
    }
}
