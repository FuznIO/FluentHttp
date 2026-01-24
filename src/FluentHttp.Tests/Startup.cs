namespace FluentHttp.Tests;

[TestClass]
public class Startup : IStartup
{
    [AssemblyInitialize]
    public static async Task Init(TestContext testContext)
    {
        await TestFuznIntegration.Init(testContext);
    }

    [AssemblyCleanup]
    public static async Task Cleanup(TestContext testContext)
    {
        await TestFuznIntegration.Cleanup(testContext);
    }

    public void Configure(TestFuznConfiguration configuration)
    {
    }
}
