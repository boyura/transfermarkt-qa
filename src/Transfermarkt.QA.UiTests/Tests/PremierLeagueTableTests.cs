using Transfermarkt.QA.UiTests.Core;
using Transfermarkt.QA.Framework.Core;
using Transfermarkt.QA.UiTests.Mocks;
using Transfermarkt.QA.UiTests.Pages;
using Transfermarkt.QA.UiTests.Pages.Components;

namespace Transfermarkt.QA.UiTests.Tests;

[TestFixture, Category("E2E")]
public class PremierLeagueTableTests : TransfermarktUiTest
{
    private const int ExpectedRows = 10;

    private HomePage _home = null!;

    // Page is created per test by PageTest, so the page object is built once per test here, not inside each test
    [SetUp]
    public void CreatePages() => _home = new HomePage(Page);

    /// <summary>Opens the home page and brings the lazily rendered table widget into view.</summary>
    private async Task<PremierLeagueTableComponent> OpenTableAsync()
    {
        await _home.OpenAsync();
        var table = _home.PremierLeagueTable;
        await table.ScrollIntoViewAsync();
        return table;
    }

    private static readonly Regex Number = new(@"^-?\d+$");
    private static readonly Regex UnsignedNumber = new(@"^\d+$");
    private static readonly Regex CompleteTableHref = new(@"/premier-league/tabelle/wettbewerb/GB1");

    [Test(Description = "Premier League table widget: headers, 10 rows, club logos, club names and numeric Matches/GD/Pts")]
    public async Task Home_PremierLeagueTable_HasExpectedStructureAndData()
    {
        var table = await OpenTableAsync();

        // Headers (the 2nd column, the trend icon, has no title)
        await Expect(table.Headers).ToHaveTextAsync(["#", "", "club", "Matches", "GD", "Pts"]);

        // Exactly 10 club rows and 10 logos
        await Expect(table.Rows).ToHaveCountAsync(ExpectedRows);
        await Expect(table.ClubLogos).ToHaveCountAsync(ExpectedRows);

        for (var i = 0; i < ExpectedRows; i++)
        {
            var row = table.Row(i);

            await Expect(row.ClubLogo).ToBeVisibleAsync();
            await Expect(row.ClubName).Not.ToBeEmptyAsync();
            await Expect(row.Matches).ToHaveTextAsync(UnsignedNumber);
            await Expect(row.GoalDifference).ToHaveTextAsync(Number); // may be negative
            await Expect(row.Points).ToHaveTextAsync(UnsignedNumber);
        }

        // Clicking it opens the full Premier League table
        await Expect(table.CompleteTableLink).ToHaveTextAsync("To the complete table");
        await table.CompleteTableLink.ClickAsync();
        await Expect(Page).ToHaveURLAsync(CompleteTableHref); // same tab navigated
        Assert.That(Context.Pages, Has.Count.EqualTo(1), "The link must not open a new tab.");
    }

    [Test(Description = "Trend icon follows the backend: no trend shows no arrow, 'up' an up arrow, 'down' a down arrow, for the right club")]
    public async Task Home_PremierLeagueTable_TrendIconFollowsBackendResponse()
    {
        var api = new PremierLeagueTableApiMock(Page);
        await api.SetTrendsAsync(Trend.None, Trend.Up, Trend.Down);

        var table = await OpenTableAsync();
        await Expect(table.Rows.First).ToBeVisibleAsync(); // rows render only after the intercepted response arrives

        var noTrend = table.RowOf(api.ClubWith(Trend.None));
        await Expect(noTrend.Arrow).ToBeHiddenAsync();

        var up = table.RowOf(api.ClubWith(Trend.Up));
        await Expect(up.UpArrow).ToBeVisibleAsync();
        await Expect(up.DownArrow).ToBeHiddenAsync();

        var down = table.RowOf(api.ClubWith(Trend.Down));
        await Expect(down.DownArrow).ToBeVisibleAsync();
        await Expect(down.UpArrow).ToBeHiddenAsync();
    }
}
