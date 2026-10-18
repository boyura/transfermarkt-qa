namespace Transfermarkt.QA.UiTests.Pages.Components;

/// <summary>
/// One result group on the search results page (clubs, players, agents...), found by the title in its heading,
/// e.g. "Search results: Clubs - 6 Hits".
/// </summary>
public sealed class SearchResultGroup(IPage page, string title)
{
    private ILocator Box => page.Locator("div.box").Filter(new() { Has = Heading }).First;

    public ILocator Heading =>
        page.Locator("h2.content-box-headline").Filter(new() { HasTextRegex = new(Regex.Escape(title), RegexOptions.IgnoreCase) }).First;

    public ILocator Rows => Box.Locator("table.items > tbody > tr");

    /// <summary>Name links (first column) of the rows on the current page.</summary>
    public ILocator Names => Rows.Locator("td.hauptlink a");

    /// <summary>Total hits shown in the heading ("... - 6 Hits").</summary>
    public async Task<int> HitsAsync()
    {
        var match = Regex.Match(await Heading.InnerTextAsync(), @"(\d+)\s*hits", RegexOptions.IgnoreCase);
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }
}
