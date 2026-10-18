using Transfermarkt.QA.UiTests.Pages.Components;

namespace Transfermarkt.QA.UiTests.Pages;

public sealed class SearchResultsPage(IPage page) : BasePage(page)
{
    public SearchResultGroup Clubs => new(Page, "Clubs");
}
