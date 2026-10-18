using System.Text.Json;

namespace Transfermarkt.QA.Framework.Config;

/// <summary>
/// Single source of truth for run configuration, read from appsettings.json. When the environment
/// variable Environment=&lt;name&gt; is set (e.g. stage), keys in appsettings.&lt;name&gt;.json override the base file.
/// Playwright's own switches (HEADED=1, BROWSER=..., PWDEBUG=1) are separate and still use env vars.
/// </summary>
public static class TestSettings
{
    private static readonly Dictionary<string, JsonElement> Values = Load();

    public static string BaseUrl => Values["BaseUrl"].GetString()!;

    /// <summary>A regular desktop UA: the default "HeadlessChrome" UA is commonly blocked by bot protection.</summary>
    public static string UserAgent => Values["UserAgent"].GetString()!;

    public static float ExpectTimeoutMs => Values["ExpectTimeoutMs"].GetSingle();
    public static float ActionTimeoutMs => Values["ActionTimeoutMs"].GetSingle();
    public static float NavigationTimeoutMs => Values["NavigationTimeoutMs"].GetSingle();

    /// <summary>Where failure screenshots and traces are written: an "artifacts" folder next to the test binaries.</summary>
    public static string ArtifactsDir => Path.Combine(TestContext.CurrentContext.WorkDirectory, "artifacts");

    private static Dictionary<string, JsonElement> Load()
    {
        var values = Read("appsettings.json");

        var environment = Environment.GetEnvironmentVariable("Environment");
        if (!string.IsNullOrWhiteSpace(environment))
        {
            foreach (var (key, value) in Read($"appsettings.{environment}.json"))
                values[key] = value;
        }

        return values;
    }

    private static Dictionary<string, JsonElement> Read(string fileName) =>
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, fileName)))!;
}
