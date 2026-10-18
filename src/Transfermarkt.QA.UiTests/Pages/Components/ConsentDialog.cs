namespace Transfermarkt.QA.UiTests.Pages.Components;

/// <summary>Sourcepoint cookie consent in an iframe overlay; accepted once per run by <see cref="ConsentState"/>.</summary>
public sealed class ConsentDialog(IPage page)
{
    // "Accept & continue" button inside the consent notice (the button itself has no id)
    private ILocator AcceptButton => page.Locator("iframe[id^='sp_message_iframe']").First.ContentFrame.Locator("#notice .accept-all");

    public Task AcceptAsync() => AcceptButton.ClickAsync();
}
