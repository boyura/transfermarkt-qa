namespace Transfermarkt.QA.UiTests.Pages.Components;

/// <summary>
/// Cookie consent (Sourcepoint CMP) renders inside an iframe overlay that intercepts clicks.
/// </summary>
public sealed class ConsentDialog(IPage page)
{
    private const string FrameSelector = "iframe[id^='sp_message_iframe'], iframe[title*='consent' i]";

    private ILocator Overlay => page.Locator(FrameSelector);

    // "Accept & continue" button inside the consent notice (the button itself has no id)
    private ILocator AcceptButton => page.FrameLocator(FrameSelector).First.Locator("#notice .accept-all");

    public Task RegisterAutoDismissAsync() =>
        page.AddLocatorHandlerAsync(Overlay, async () => await AcceptButton.First.ClickAsync());
}
