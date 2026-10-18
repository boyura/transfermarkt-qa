namespace Transfermarkt.QA.UiTests.Pages.Components;

public sealed class SearchComponent(IPage page)
{
    public ILocator Form => page.Locator("form#schnellsuche");

    public ILocator Input => Form.Locator("input[name='query']");

    public ILocator Button => Form.Locator("button[type='submit']");

    public async Task SearchAsync(string query)
    {
        await Input.FillAsync(query);
        await Button.ClickAsync();
    }
}
