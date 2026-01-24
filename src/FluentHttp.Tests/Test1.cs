using System.Threading.Tasks;

namespace FluentHttp.Tests;

[TestClass]
public sealed class Test1 : Test
{
    [Test]
    public async Task TestMethod1()
    {
        await Scenario()
            .Step("Step 1", async context =>
            {

            })
            .Run();
    }
}
