using NUnit.Framework.Interfaces;
using Transfermarkt.QA.Framework.Config;

namespace Transfermarkt.QA.Framework.Core;

/// <summary>
/// Base for browser tests. On top of PageTest's isolated context per test it adds fixed context options, timeouts,
/// HTTP 500 monitoring, and a screenshot + trace attached to the result on failure.
/// </summary>
public abstract class BaseUiTest : PageTest
{
    protected HttpErrorMonitor Http { get; private set; } = null!;

    /// <summary>Storage state JSON (cookies + local storage) every test context starts from; null for a clean context.</summary>
    protected virtual string? StorageState => null;

    public override BrowserNewContextOptions ContextOptions()
    {
        var options = DefaultContextOptions();
        options.StorageState = StorageState;
        return options;
    }

    /// <summary>Static so a one-time setup outside a test creates its context the same way.</summary>
    public static BrowserNewContextOptions DefaultContextOptions() => new()
    {
        BaseURL = TestSettings.BaseUrl,
        Locale = "en-GB",
        TimezoneId = "Europe/London",
        ViewportSize = new ViewportSize { Width = 1440, Height = 900 },
        UserAgent = TestSettings.UserAgent
    };

    [SetUp]
    public async Task UiSetUp()
    {
        Assertions.SetDefaultExpectTimeout(TestSettings.ExpectTimeoutMs);
        Page.SetDefaultTimeout(TestSettings.ActionTimeoutMs);
        Page.SetDefaultNavigationTimeout(TestSettings.NavigationTimeoutMs);

        Http = new HttpErrorMonitor(Context);

        await Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = false });
    }

    /// <summary>Fail on HTTP 500s from MonitoredHosts. Override to false only in tests that inject a 500 on purpose.</summary>
    protected virtual bool FailOn500 => true;

    [TearDown]
    public async Task UiTearDown()
    {
        Http.Dispose();
        TestContext.Out.WriteLine(Http.Log.Describe());

        var http500s = FailOn500 ? Http.Log.Failures : [];
        var failed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed || http500s.Count > 0;
        if (!failed)
        {
            await Context.Tracing.StopAsync(); // discard: keeps green runs fast and small
            return;
        }

        await SaveDiagnosticsAsync();

        // Asserted last so the screenshot/trace are captured even when this is the only failure.
        Assert.That(http500s, Is.Empty, "HTTP 500 responses were observed during the test");
    }

    private async Task SaveDiagnosticsAsync()
    {
        Directory.CreateDirectory(TestSettings.ArtifactsDir);
        var name = SafeFileName(TestContext.CurrentContext.Test.Name);

        var screenshot = Path.Combine(TestSettings.ArtifactsDir, $"{name}.png");
        var trace = Path.Combine(TestSettings.ArtifactsDir, $"{name}.trace.zip");

        await Page.ScreenshotAsync(new() { Path = screenshot, FullPage = true });
        await Context.Tracing.StopAsync(new() { Path = trace });

        TestContext.AddTestAttachment(screenshot, "Failure screenshot");
        TestContext.AddTestAttachment(trace, "Playwright trace (npx playwright show-trace <file>)");
    }

    private static string SafeFileName(string value)
    {
        var cleaned = string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) || c is '"' or ' ' ? '_' : c));
        return cleaned.Length > 100 ? cleaned[..100] : cleaned;
    }
}
