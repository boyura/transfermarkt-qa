using Transfermarkt.QA.Framework.Config;

namespace Transfermarkt.QA.UiTests.Pages.Components;

/// <summary>Top-level sections of the main navigation bar.</summary>
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

/// <summary>Main navigation bar: sections are clicked by their visible label, like a user would.</summary>
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

    /// <summary>The highlighted entry (the site marks it with the <c>active</c> class); exactly one is expected.</summary>
    public ILocator ActiveItem => Bar.Locator("a.active");

    public Task ClickAsync(NavSection section) => Item(section).ClickAsync();

    /// <summary>Visible label of the section, for asserting which item is active.</summary>
    public static string Label(NavSection section) => Sections[section].Label;

    /// <summary>URL that ends with exactly the section path (e.g. "/navigation/gaming"), nothing after it. Discover is the bare host plus "/".</summary>
    public static Regex UrlPattern(NavSection section) =>
        section == NavSection.Discover
            ? new Regex(Regex.Escape(TestSettings.BaseUrl + "/") + "$")
            : new Regex(Regex.Escape(Sections[section].Path) + "$");
}
