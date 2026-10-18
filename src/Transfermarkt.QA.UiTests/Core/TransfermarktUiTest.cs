using Transfermarkt.QA.Framework.Core;
using Transfermarkt.QA.UiTests.Pages.Components;

namespace Transfermarkt.QA.UiTests.Core;

/// <summary>Base for all Transfermarkt UI tests: adds the site's cookie consent handling to <see cref="BaseUiTest"/>.</summary>
public abstract class TransfermarktUiTest : BaseUiTest
{
    // NUnit runs the base-class [SetUp] first. A locator handler fires only when the overlay blocks
    // an action, so tests need no "if banner then click" branching and no fixed sleeps.
    [SetUp]
    public Task DismissConsentAutomatically() => new ConsentDialog(Page).RegisterAutoDismissAsync();
}
