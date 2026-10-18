using System.Text.Json;

namespace Transfermarkt.QA.Framework.Config;

/// <summary>
/// Run configuration from appsettings.json; with Environment=&lt;name&gt; set, appsettings.&lt;name&gt;.json overrides it.
/// Playwright's own switches (HEADED, BROWSER, PWDEBUG) stay environment variables.
/// </summary>
public static class TestSettings
{
    private static readonly Dictionary<string, JsonElement> Values = Load();

    public static string BaseUrl => Values["BaseUrl"].GetString()!;

    /// <summary>Hosts whose HTTP 500s fail a test (exact match). Defaults to the BaseUrl host when not configured.</summary>
    public static IReadOnlyList<string> MonitoredHosts =>
        Values.TryGetValue("MonitoredHosts", out var hosts)
            ? hosts.EnumerateArray().Select(h => h.GetString()!).ToArray()
            : [new Uri(BaseUrl).Host];

    /// <summary>A regular desktop UA: the default "HeadlessChrome" UA is commonly blocked by bot protection.</summary>
    public static string UserAgent => Values["UserAgent"].GetString()!;

    public static float ExpectTimeoutMs => Values["ExpectTimeoutMs"].GetSingle();
    public static float ActionTimeoutMs => Values["ActionTimeoutMs"].GetSingle();
    public static float NavigationTimeoutMs => Values["NavigationTimeoutMs"].GetSingle();

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
