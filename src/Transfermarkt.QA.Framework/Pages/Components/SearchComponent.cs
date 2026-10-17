namespace Transfermarkt.QA.Tests.Pages.Components;

/// <summary>Header quick-search: the form with its input and the search icon button.</summary>
public sealed class SearchComponent(IPage page)
{
    public ILocator Form => page.Locator("form#schnellsuche");

    public ILocator Input => Form.Locator("input[name='query']");

    public ILocator Button => Form.Locator("button[type='submit']");

    /// <summary>Types the query and submits it by clicking the search icon.</summary>
    public async Task SearchAsync(string query)
    {
        await Input.FillAsync(query);
        await Button.ClickAsync();
    }
}
