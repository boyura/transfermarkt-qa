using Transfermarkt.QA.Tests.Pages.Components;

namespace Transfermarkt.QA.Tests.Pages;

/// <summary>
/// Page objects expose intent (what a user does / sees) and own all locators.
/// They never contain assertions about business expectations: tests do.
/// </summary>
public abstract class BasePage
{
    protected IPage Page { get; }
    public HeaderComponent Header { get; }

    protected BasePage(IPage page)
    {
        Page = page;
        Header = new HeaderComponent(page);
    }
}
