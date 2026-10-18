namespace Transfermarkt.QA.UiTests.Pages.Components;

/// <summary>One club row of the standings widget. Cell order (verified): position, trend icon, club, Matches, GD, Pts.</summary>
public sealed class Row(ILocator row)
{
    private const string ArrowSelector = "span.arrow";

    private ILocator Cell(int column) => row.Locator(":scope > div").Nth(column);

    public ILocator Position => Cell(0);

    /// <summary>Trend cell (2nd column): holds <c>span.arrow.up</c> or <c>span.arrow.down</c>; for no trend the span has no "arrow" class.</summary>
    public ILocator Arrow => row.Locator(ArrowSelector);
    public ILocator UpArrow => row.Locator($"{ArrowSelector}.up");
    public ILocator DownArrow => row.Locator($"{ArrowSelector}.down");
    public ILocator ClubLogo => row.Locator(PremierLeagueTableComponent.ClubLogoSelector);
    public ILocator ClubName => row.Locator(".club .name");
    public ILocator Matches => Cell(3);
    public ILocator GoalDifference => Cell(4);
    public ILocator Points => Cell(5);
}
