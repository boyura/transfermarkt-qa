using Transfermarkt.QA.Tests.Helpers;

namespace Transfermarkt.QA.Tests.Pages;

/// <summary>
/// Home page. All locators are private: tests talk to the page through intent-level methods
/// (<c>Expect*</c> methods wrap Playwright web-first assertions, <c>Read*</c> methods return plain data).
/// </summary>
public sealed class HomePage(IPage page) : BasePage(page)
{
    private static readonly Regex Number = new(@"^-?\d+$", RegexOptions.Compiled);
    private static readonly Regex UnsignedNumber = new(@"^\d+$", RegexOptions.Compiled);

    /// <summary>Verified: the "Premier League" home section contains a matchday results table (Date / Home team / Away team).</summary>
    private ILocator PremierLeagueHeadingLink => Page.Locator("h2.direct-headline__header a[href*='/wettbewerb/GB1']").First;
    private ILocator MatchdayTable => Page.Locator("table.startseite")
        .Filter(new() { Has = Page.GetByRole(AriaRole.Columnheader, new() { Name = "Home team" }) }).First;
    private ILocator MatchdayRows => MatchdayTable.Locator("tbody tr");

    /// <summary>
    /// Verified on the live site: the home "Table" widget (Premier League, competition-id 112) is the lazy
    /// <c>&lt;tm-competition-tables&gt;</c> Svelte component. It is NOT a &lt;table&gt;: a header &lt;section&gt; and one
    /// <c>section[data-key]</c> per club, each holding div cells: position, trend icon, club (logo + name), Matches, GD, Pts.
    /// Playwright CSS pierces its open shadow root.
    /// </summary>
    private ILocator StandingsWidget => Page.Locator("tm-competition-tables[competition-id='112']");
    private ILocator StandingsHeaders => StandingsWidget.Locator("section > header > div");
    private ILocator StandingsRows => StandingsWidget.Locator("section[data-key]");
    private ILocator StandingsLogos => StandingsRows.Locator(".club img.logo");

    private ILocator Cell(int row, int column) => StandingsRows.Nth(row).Locator(":scope > div").Nth(column);
    private ILocator ClubLogo(int row) => StandingsRows.Nth(row).Locator(".club img.logo");
    private ILocator ClubName(int row) => StandingsRows.Nth(row).Locator(".club .name");

    private const int PositionColumn = 0, MatchesColumn = 3, GoalDifferenceColumn = 4, PointsColumn = 5;

    public async Task OpenAsync() =>
        await Page.GotoAsync("/", new() { WaitUntil = WaitUntilState.DOMContentLoaded });

    // ---- Matchday fixtures -------------------------------------------------------------------

    public async Task ExpectMatchdayTableShownAsync()
    {
        await Assertions.Expect(PremierLeagueHeadingLink).ToBeVisibleAsync();
        await Assertions.Expect(MatchdayTable).ToBeVisibleAsync();
        await Assertions.Expect(MatchdayRows).Not.ToHaveCountAsync(0);
    }

    public async Task<IReadOnlyList<(string Home, string Away)>> ReadMatchdayFixturesAsync()
    {
        var raw = await MatchdayRows.EvaluateAllAsync<string[][]>(
            "rows => rows.map(r => Array.from(r.querySelectorAll('td')).map(c => c.innerText.trim()).filter(x => x.length))");
        // Row shape (verified): [date?] home, score|kickoff, away
        return raw.Where(c => c.Length >= 3).Select(c => (c[^3], c[^1])).ToList();
    }

    // ---- Standings widget --------------------------------------------------------------------

    /// <summary>The widget renders lazily: bring it into view, then rely on web-first waiting.</summary>
    public async Task ScrollToStandingsAsync()
    {
        await StandingsWidget.ScrollIntoViewIfNeededAsync();
        await Assertions.Expect(StandingsRows.First).ToBeVisibleAsync();
    }

    /// <summary>Header titles in column order; the trend-icon column has an empty title.</summary>
    public async Task ExpectStandingsHeadersAsync(params string[] headers) =>
        await Assertions.Expect(StandingsHeaders).ToHaveTextAsync(
            headers.Select(h => new Regex($"^{Regex.Escape(h)}$", RegexOptions.IgnoreCase)).ToArray());

    public async Task ExpectStandingsRowCountAsync(int count)
    {
        await Assertions.Expect(StandingsRows).ToHaveCountAsync(count);
        await Assertions.Expect(StandingsLogos).ToHaveCountAsync(count);
    }

    public async Task ExpectStandingsNotEmptyAsync() =>
        await Assertions.Expect(StandingsRows).Not.ToHaveCountAsync(0);

    public async Task<int> CountStandingsRowsAsync() => await StandingsRows.CountAsync();

    public async Task ExpectPositionAsync(int row, int position) =>
        await Assertions.Expect(Cell(row, PositionColumn)).ToHaveTextAsync(position.ToString());

    public async Task ExpectClubLogoVisibleAsync(int row) =>
        await Assertions.Expect(ClubLogo(row)).ToBeVisibleAsync();

    public async Task ExpectClubNameNotEmptyAsync(int row) =>
        await Assertions.Expect(ClubName(row)).Not.ToBeEmptyAsync();

    public async Task ExpectMatchesNumericAsync(int row) =>
        await Assertions.Expect(Cell(row, MatchesColumn)).ToHaveTextAsync(UnsignedNumber);

    /// <summary>Goal difference may be negative.</summary>
    public async Task ExpectGoalDifferenceNumericAsync(int row) =>
        await Assertions.Expect(Cell(row, GoalDifferenceColumn)).ToHaveTextAsync(Number);

    public async Task ExpectPointsNumericAsync(int row) =>
        await Assertions.Expect(Cell(row, PointsColumn)).ToHaveTextAsync(UnsignedNumber);

    /// <summary>Reads the standings into typed rows (position, club, points) for cross-row invariant checks.</summary>
    public async Task<IReadOnlyList<LeagueTableRow>> ReadStandingsAsync()
    {
        var count = await StandingsRows.CountAsync();
        var rows = new List<LeagueTableRow>();
        for (var i = 0; i < count; i++)
        {
            rows.Add(new LeagueTableRow(
                int.Parse(await Cell(i, PositionColumn).InnerTextAsync()),
                (await ClubName(i).InnerTextAsync()).Trim(),
                int.Parse(await Cell(i, PointsColumn).InnerTextAsync())));
        }
        return rows;
    }
}
