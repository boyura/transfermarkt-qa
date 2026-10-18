using Transfermarkt.QA.Framework.Config;

namespace Transfermarkt.QA.UiTests.Pages.Components;

/// <summary>
/// Premier League standings widget on the home page: a lazy <c>&lt;tm-competition-tables&gt;</c> Svelte component, not a
/// &lt;table&gt;. One <c>section[data-key]</c> per club; Playwright CSS pierces its open shadow root.
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

    /// <summary>
    /// The widget requests its data only once it enters the viewport. Right after load the consent overlay closes and ads
    /// render, shifting the layout, so a single scroll can miss and the rows never load: re-scroll until the first row shows.
    /// </summary>
    public async Task ScrollIntoViewAsync()
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(TestSettings.ExpectTimeoutMs);
        while (true)
        {
            await Root.ScrollIntoViewIfNeededAsync();
            try
            {
                await Rows.First.WaitForAsync(new() { Timeout = 2000 });
                return;
            }
            catch (TimeoutException) when (DateTime.UtcNow < deadline)
            {
                // not rendered yet: the layout probably moved, scroll again
            }
        }
    }
}
