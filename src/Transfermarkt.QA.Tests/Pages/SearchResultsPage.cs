using Transfermarkt.QA.Tests.Pages.Components;

namespace Transfermarkt.QA.Tests.Pages;

/// <summary>Quick-search results page. Exposes locators only: tests own the <c>Expect</c> checks.</summary>
public sealed class SearchResultsPage(IPage page) : BasePage(page)
{
    public SearchResultGroup Clubs => new(Page, "Clubs");
}
