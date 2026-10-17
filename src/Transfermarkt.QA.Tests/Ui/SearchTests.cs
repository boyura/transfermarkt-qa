using Transfermarkt.QA.Tests.Core;
using Transfermarkt.QA.Tests.Pages;

namespace Transfermarkt.QA.Tests.Ui;

[TestFixture, Category("E2E")]
public class SearchTests : BaseUiTest
{
    private const string SearchTerm = "manchester city";

    private HomePage _home = null!;

    [SetUp]
    public void CreatePages() => _home = new HomePage(Page);

    private static readonly Regex SearchResultsUrl = new(@"/schnellsuche/ergebnis/schnellsuche\?.*query=manchester(\+|%20)city", RegexOptions.IgnoreCase);
    private static readonly Regex TermRegex = new(SearchTerm, RegexOptions.IgnoreCase);

    [Test(Description = "Header search: querying a known club opens the results page with a non-empty Clubs group containing the term")]
    public async Task Header_Search_KnownClub_ShowsMatchingClubs()
    {
        await _home.OpenAsync();

        await _home.Header.Search.SearchAsync(SearchTerm);

        await Expect(Page).ToHaveURLAsync(SearchResultsUrl);

        var clubs = new SearchResultsPage(Page).Clubs;

        // The search box keeps the searched term
        await Expect(_home.Header.Search.Input).ToHaveValueAsync(TermRegex);

        // Clubs heading reports hits: "Search results: Clubs - N Hits", N > 0
        await Expect(clubs.Heading).ToHaveTextAsync(new Regex(@"[Ss]earch results: [Cc]lubs\s*-\s*\d+\s*[Hh]its"));
        Assert.That(await clubs.HitsAsync(), Is.GreaterThan(0), "Clubs heading reports no hits.");

        // The clubs list is not empty and every club name contains the searched term
        await Expect(clubs.Rows.First).ToBeVisibleAsync();
        var rows = await clubs.Rows.CountAsync();
        Assert.That(rows, Is.GreaterThan(0), "No club rows listed.");
        await Expect(clubs.Names).ToHaveCountAsync(rows); // every row has a name link
        await Expect(clubs.Names.Filter(new() { HasNotTextRegex = TermRegex }))
            .ToHaveCountAsync(0, new() { Timeout = 2_000 }); // no club without the term
    }
}
