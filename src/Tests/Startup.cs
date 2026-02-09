using Fuzn.TestFuzn;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class Startup : IStartup, IBeforeSuite, IAfterSuite
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

    public Task BeforeSuite(Context context)
    {
        SuiteData.Init();
        return Task.CompletedTask;
    }

    public Task AfterSuite(Context context)
    {
        SuiteData.ServiceProvider?.Dispose();
        return Task.CompletedTask;
    }
}
