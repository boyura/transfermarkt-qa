using Transfermarkt.QA.Tests.Pages.Components;

namespace Transfermarkt.QA.Tests.Pages;

/// <summary>
/// Home page. Exposes locators and components (no assertions here): tests own the <c>Expect</c> checks.
/// </summary>
public sealed class HomePage(IPage page) : BasePage(page)
{
    /// <summary>Premier League standings widget.</summary>
    public PremierLeagueTableComponent PremierLeagueTable => new(Page);

    public async Task OpenAsync() =>
        await Page.GotoAsync("/", new() { WaitUntil = WaitUntilState.DOMContentLoaded });

}
