namespace Transfermarkt.QA.Tests.Pages.Components;

/// <summary>Global site header: logo, main navigation, search, login entry point.</summary>
public sealed class HeaderComponent(IPage page)
{
    public ILocator Logo => page.Locator("a.tm-header__logo").First;

    public SearchComponent Search => new(page);

    /// <summary>Verified on the live site: main nav is <c>nav.main-navbar</c> with links such as /navigation/marktwerte.</summary>
    public ILocator NavLink(string href) =>
        page.Locator($"nav.main-navbar a[href='{href}']").First;

    public ILocator LoginLink =>
        page.GetByRole(AriaRole.Link, new() { NameRegex = new Regex(@"^\s*(log ?in|sign in)\s*$", RegexOptions.IgnoreCase) }).First;

    public ILocator LogoutLink =>
        page.GetByRole(AriaRole.Link, new() { NameRegex = new Regex(@"log ?out|sign out", RegexOptions.IgnoreCase) });
}
