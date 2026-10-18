namespace Transfermarkt.QA.UiTests.Pages.Components;

public sealed class HeaderComponent(IPage page)
{
    public ILocator Logo => page.Locator("a.tm-header__logo").First;

    public SearchComponent Search => new(page);

    public NavigationComponent Navigation => new(page);

    /// <summary>"Log in" entry point: a button inside the tm-userbox web component.</summary>
    public ILocator LoginButton =>
        page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex(@"^\s*(log ?in|sign in)\s*$", RegexOptions.IgnoreCase) }).First;

    public LoginFormComponent LoginForm => new(page);

    public ILocator LogoutLink =>
        page.GetByRole(AriaRole.Link, new() { NameRegex = new Regex(@"log ?out|sign out", RegexOptions.IgnoreCase) });
}
