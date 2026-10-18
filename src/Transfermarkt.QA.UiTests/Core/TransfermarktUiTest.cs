using Transfermarkt.QA.Framework.Core;

namespace Transfermarkt.QA.UiTests.Core;

/// <summary>Base for all Transfermarkt UI tests: each test starts with cookie consent already given (<see cref="ConsentState"/>).</summary>
public abstract class TransfermarktUiTest : BaseUiTest
{
    protected override string? StorageState => ConsentState.Json;
}
