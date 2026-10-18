using Transfermarkt.QA.UiTests.Core;
using Transfermarkt.QA.Framework.Core;
using Transfermarkt.QA.UiTests.Pages;
using Transfermarkt.QA.UiTests.Pages.Components;

namespace Transfermarkt.QA.UiTests.Tests;

[TestFixture, Category("E2E")]
public class NavigationTests : TransfermarktUiTest
{
    private HomePage _home = null!;

    private NavigationComponent Nav => _home.Header.Navigation;

    [SetUp]
    public void CreatePages() => _home = new HomePage(Page);

    [Test(Description = "Main navigation: each section opens its page and becomes the active item; the logo returns to Discover")]
    public async Task Main_Navigation_Flow()
    {
        await _home.OpenAsync();
        await Expect(Nav.Item(NavSection.Discover)).ToHaveClassAsync(NavigationComponent.ActiveClass);

        foreach (var section in Enum.GetValues<NavSection>().Where(s => s != NavSection.Discover))
        {
            await Nav.ClickAsync(section);

            await Expect(Page).ToHaveURLAsync(NavigationComponent.UrlPattern(section));
            await Expect(Nav.Item(section)).ToHaveClassAsync(NavigationComponent.ActiveClass);
        }

        await _home.Header.Logo.ClickAsync();

        await Expect(Page).ToHaveURLAsync(NavigationComponent.UrlPattern(NavSection.Discover));
        await Expect(Nav.Item(NavSection.Discover)).ToHaveClassAsync(NavigationComponent.ActiveClass);
    }
}
