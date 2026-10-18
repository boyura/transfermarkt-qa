using NUnit.Framework.Interfaces;
using Transfermarkt.QA.Framework.Config;

namespace Transfermarkt.QA.Framework.Core;

/// <summary>
/// Base for browser tests. Playwright's PageTest already gives every test a fresh,
/// isolated BrowserContext + Page (no shared state between tests). On top of that this adds:
/// consistent context options, timeouts, an HTTP error monitor, and failure diagnostics (screenshot + trace) attached to the NUnit result.
/// </summary>
public abstract class BaseUiTest : PageTest
{
    protected HttpErrorMonitor Http { get; private set; } = null!;

    public override BrowserNewContextOptions ContextOptions() => new()
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

    /// <summary>
    /// Every test fails if any HTTP 500 came from a host in MonitoredHosts (third-party ads/trackers are ignored). Override to false
    /// only for tests that inject a 500 on purpose.
    /// </summary>
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
