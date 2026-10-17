namespace Transfermarkt.QA.Tests.Config;

/// <summary>
/// Single source of truth for run configuration. Everything is overridable via
/// environment variables so the same build runs locally, headless and in CI.
/// Playwright's own switches also apply: HEADED=1, BROWSER=chromium|firefox|webkit, PWDEBUG=1.
/// </summary>
public static class TestSettings
{
    private const string DefaultUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) " +
        "Chrome/131.0.0.0 Safari/537.36";

    public static string BaseUrl => (Env("BASE_URL") ?? "https://www.transfermarkt.com");

    /// <summary>A regular desktop UA: the default "HeadlessChrome" UA is commonly blocked by bot protection.</summary>
    public static string UserAgent => Env("USER_AGENT") ?? DefaultUserAgent;

    public static float ExpectTimeoutMs => EnvFloat("EXPECT_TIMEOUT_MS", 10_000);
    public static float ActionTimeoutMs => EnvFloat("ACTION_TIMEOUT_MS", 15_000);
    public static float NavigationTimeoutMs => EnvFloat("NAVIGATION_TIMEOUT_MS", 45_000);

    public static string ArtifactsDir =>
        Env("ARTIFACTS_DIR") ?? Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts");

    private static string? Env(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static float EnvFloat(string name, float fallback) =>
        float.TryParse(Env(name), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var parsed) && parsed > 0
            ? parsed
            : fallback;
}
