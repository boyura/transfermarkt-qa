using Transfermarkt.QA.Tests.Core;
using Transfermarkt.QA.Tests.Pages;

namespace Transfermarkt.QA.Tests.Ui;

[TestFixture, Category("E2E")]
public class PremierLeagueTableTests : BaseUiTest
{
    private const int ExpectedRows = 10;

    private HomePage _home = null!;

    // Page is created per test by PageTest, so the page object is built once per test here, not inside each test
    [SetUp]
    public void CreatePages() => _home = new HomePage(Page);

    private static readonly Regex Number = new(@"^-?\d+$");
    private static readonly Regex UnsignedNumber = new(@"^\d+$");
    private static readonly Regex CompleteTableHref = new(@"/premier-league/tabelle/wettbewerb/GB1");

    [Test(Description = "Premier League table widget: headers, 10 rows, club logos, club names and numeric Matches/GD/Pts")]
    public async Task Home_PremierLeagueTable_HasExpectedStructureAndData()
    {
        await _home.OpenAsync();
        var table = _home.PremierLeagueTable;
        await table.ScrollIntoViewAsync();

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
}
