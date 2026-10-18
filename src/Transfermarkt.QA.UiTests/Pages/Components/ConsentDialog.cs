namespace Transfermarkt.QA.UiTests.Pages.Components;

/// <summary>
/// Cookie consent (Sourcepoint CMP) renders inside an iframe overlay that intercepts clicks.
/// </summary>
public sealed class ConsentDialog(IPage page)
{
    private const string FrameSelector = "iframe[id^='sp_message_iframe'], iframe[title*='consent' i]";

    private ILocator Overlay => page.Locator(FrameSelector);

    private ILocator AcceptButton => page.FrameLocator(FrameSelector).First
        .GetByRole(AriaRole.Button, new()
        {
            NameRegex = new Regex("accept( &| and)? continue|accept all|agree", RegexOptions.IgnoreCase)
        });

    public Task RegisterAutoDismissAsync() =>
        page.AddLocatorHandlerAsync(Overlay, async () => await AcceptButton.First.ClickAsync());
}
