using Transfermarkt.QA.Framework.Config;

namespace Transfermarkt.QA.UiTests.Pages.Components;

public enum NavSection
{
    Discover,
    TransfersAndRumours,
    MarketValues,
    Competitions,
    Statistics,
    Forum,
    Gaming
}

public sealed class NavigationComponent(IPage page)
{
    private static readonly Dictionary<NavSection, (string Label, string Path)> Sections = new()
    {
        [NavSection.Discover] = ("DISCOVER", "/"),
        [NavSection.TransfersAndRumours] = ("TRANSFERS & RUMOURS", "/navigation/transfersundgeruechte"),
        [NavSection.MarketValues] = ("MARKET VALUES", "/navigation/marktwerte"),
        [NavSection.Competitions] = ("COMPETITIONS", "/navigation/wettbewerbe"),
        [NavSection.Statistics] = ("STATISTICS", "/navigation/statistiken"),
        [NavSection.Forum] = ("FORUM", "/navigation/community"),
        [NavSection.Gaming] = ("GAMING", "/navigation/gaming")
    };

    private ILocator Bar => page.Locator("nav.main-navbar");

    public ILocator Item(NavSection section) =>
        Bar.GetByRole(AriaRole.Link, new() { Name = Sections[section].Label, Exact = true });

    public ILocator ActiveItems => Bar.Locator("a.active");

    /// <summary>The whole <c>active</c> class among others, not a substring like "inactive".</summary>
    public static readonly Regex ActiveClass = new(@"(^|\s)active(\s|$)");

    public Task ClickAsync(NavSection section) => Item(section).ClickAsync();

    /// <summary>URL that ends with exactly the section path (e.g. "/navigation/gaming"), nothing after it. Discover is the bare host plus "/".</summary>
    public static Regex UrlPattern(NavSection section) =>
        section == NavSection.Discover
            ? new Regex(Regex.Escape(TestSettings.BaseUrl + "/") + "$")
            : new Regex(Regex.Escape(Sections[section].Path) + "$");
}
