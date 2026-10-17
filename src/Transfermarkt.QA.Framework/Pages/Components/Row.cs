namespace Transfermarkt.QA.Tests.Pages.Components;

/// <summary>One club row of the standings widget. Cell order (verified): position, trend icon, club, Matches, GD, Pts.</summary>
public sealed class Row(ILocator row)
{
    private ILocator Cell(int column) => row.Locator(":scope > div").Nth(column);

    public ILocator Position => Cell(0);
    public ILocator ClubLogo => row.Locator(PremierLeagueTableComponent.ClubLogoSelector);
    public ILocator ClubName => row.Locator(".club .name");
    public ILocator Matches => Cell(3);
    public ILocator GoalDifference => Cell(4);
    public ILocator Points => Cell(5);
}
