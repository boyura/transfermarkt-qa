using Transfermarkt.QA.Framework.Core;
using Transfermarkt.QA.UiTests.Pages.Components;

namespace Transfermarkt.QA.UiTests;

/// <summary>
/// Accepts the cookie notice once per run; every test context starts from the saved state, so the notice cannot swallow a
/// click. Collected fresh each run rather than hard-coded, because consent values expire.
/// </summary>
[SetUpFixture]
public sealed class ConsentState
{
    public static string Json { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task AcceptConsentOnce()
    {
        using var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        await using var context = await browser.NewContextAsync(BaseUiTest.DefaultContextOptions());
        var page = await context.NewPageAsync();

        await page.GotoAsync("/", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await new ConsentDialog(page).AcceptAsync();

        // The consent platform stores the choice a moment after the click; saving earlier would capture no consent.
        await page.WaitForFunctionAsync("() => document.cookie.includes('consentUUID=')");

        Json = await context.StorageStateAsync();
    }
}
