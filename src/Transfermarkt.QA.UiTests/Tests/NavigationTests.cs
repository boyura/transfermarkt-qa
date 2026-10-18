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

    /// <summary>
    /// Known site bug (README, "Assumptions and limitations"): a freshly rendered page sometimes has no highlighted item
    /// at all. That is only a warning; another item being highlighted is a real failure.
    /// </summary>
    private async Task ExpectHighlightedAsync(NavSection section)
    {
        try
        {
            await Expect(Nav.Item(section)).ToHaveClassAsync(NavigationComponent.ActiveClass);
        }
        catch (PlaywrightException)
        {
            if (await Nav.ActiveItems.CountAsync() > 0)
                throw; // another item is highlighted: a real failure

            Assert.Warn($"Known site bug: the {section} page ({Page.Url}) was served with no highlighted menu item.");
        }
    }

    [Test(Description = "Main navigation: each section opens its page and becomes the active item; the logo returns to Discover")]
    public async Task Main_Navigation_Flow()
    {
        await _home.OpenAsync();
        await ExpectHighlightedAsync(NavSection.Discover);

        foreach (var section in Enum.GetValues<NavSection>().Where(s => s != NavSection.Discover))
        {
            await Nav.ClickAsync(section);

            await Expect(Page).ToHaveURLAsync(NavigationComponent.UrlPattern(section));
            await ExpectHighlightedAsync(section);
        }

        await _home.Header.Logo.ClickAsync();

        await Expect(Page).ToHaveURLAsync(NavigationComponent.UrlPattern(NavSection.Discover));
        await ExpectHighlightedAsync(NavSection.Discover);
    }
}
