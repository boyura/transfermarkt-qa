using Transfermarkt.QA.UiTests.Pages.Components;

namespace Transfermarkt.QA.UiTests.Pages;

public sealed class HomePage(IPage page) : BasePage(page)
{
    public PremierLeagueTableComponent PremierLeagueTable => new(Page);

    public async Task OpenAsync() =>
        await Page.GotoAsync("/", new() { WaitUntil = WaitUntilState.DOMContentLoaded });

}
