namespace Transfermarkt.QA.UiTests.Pages.Components;

/// <summary>
/// The Premier League "Table" standings widget (competition-id 112) on the home page.
/// Verified on the live site: it is the lazy <c>&lt;tm-competition-tables&gt;</c> Svelte component, NOT a &lt;table&gt;:
/// a header &lt;section&gt; and one <c>section[data-key]</c> per club, each holding div cells:
/// position, trend icon, club (logo + name), Matches, GD, Pts. Playwright CSS pierces its open shadow root.
/// </summary>
public sealed class PremierLeagueTableComponent(IPage page)
{
    internal const string ClubLogoSelector = ".club img.logo";

    private ILocator Root => page.Locator("tm-competition-tables[competition-id='112']");

    public ILocator Headers => Root.Locator("section > header > div");
    public ILocator Rows => Root.Locator("section[data-key]");
    public ILocator ClubLogos => Rows.Locator(ClubLogoSelector);
    public ILocator CompleteTableLink => Root.Locator("a.content-link");

    public Row Row(int index) => new(Rows.Nth(index));

    /// <summary>The row of the club with exactly this name, so a check cannot land on the wrong club.</summary>
    public Row RowOf(string club) => new(Rows.Filter(new()
    {
        Has = page.Locator(".club .name", new() { HasTextRegex = new Regex($"^{Regex.Escape(club)}$") })
    }));

    /// <summary>The widget renders lazily: bring it into view; tests then rely on web-first waiting.</summary>
    public async Task ScrollIntoViewAsync() => await Root.ScrollIntoViewIfNeededAsync();
}
