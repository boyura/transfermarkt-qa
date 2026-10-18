using Transfermarkt.QA.Framework.Config;
using Transfermarkt.QA.Framework.Core;

namespace Transfermarkt.QA.Framework.Tests.Integration;

/// <summary>
/// Proves the HTTP 500 monitor used by the E2E tests really detects failures, so they cannot pass vacuously.
/// The 500 is injected via interception: deterministic and independent of the live site.
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
