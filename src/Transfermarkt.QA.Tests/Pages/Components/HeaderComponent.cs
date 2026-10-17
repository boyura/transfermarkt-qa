namespace Transfermarkt.QA.Tests.Pages.Components;

/// <summary>Global site header: logo, main navigation, search, login entry point.</summary>
public sealed class HeaderComponent(IPage page)
{
    public ILocator Logo => page.Locator("a.tm-header__logo").First;

    public SearchComponent Search => new(page);

    public NavigationComponent Navigation => new(page);

    public ILocator LoginLink =>
        page.GetByRole(AriaRole.Link, new() { NameRegex = new Regex(@"^\s*(log ?in|sign in)\s*$", RegexOptions.IgnoreCase) }).First;

    public ILocator LogoutLink =>
        page.GetByRole(AriaRole.Link, new() { NameRegex = new Regex(@"log ?out|sign out", RegexOptions.IgnoreCase) });
}
