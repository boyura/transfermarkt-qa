using Transfermarkt.QA.Framework.Config;
using Transfermarkt.QA.Framework.Core;

namespace Transfermarkt.QA.Framework.Tests.Integration;

/// <summary>
/// Integration level (browser + network layer): proves the HTTP error monitor behind UI test #5 actually
/// detects failures, so that test cannot pass vacuously. The 500 is injected with network interception,
/// hence this test is fully deterministic and never depends on the live site.
/// </summary>
[TestFixture, Category("Integration")]
public class NetworkInterceptionTests : BaseUiTest
{
    // This fixture injects a 500 on purpose and asserts on it itself.
    protected override bool FailOn500 => false;

    [Test(Description = "Monitor records an injected 500")]
    public async Task Monitor_DetectsInjectedServerError()
    {
        await Page.RouteAsync("**/qa-injected-500", route => route.FulfillAsync(new()
        {
            Status = 500,
            ContentType = "text/plain",
            Body = "injected failure"
        }));

        await Page.GotoAsync($"{TestSettings.BaseUrl}/qa-injected-500");

        // Delayed constraints poll the delegate (50 ms steps, 5 s cap) instead of using a fixed sleep.
        Assert.That(() => Http.Log.Failures.Select(f => f.Status).ToArray(),
            Is.EqualTo(new[] { 500 }).After(5000, 50));
    }
}
