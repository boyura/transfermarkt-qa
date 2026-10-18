using System.Collections.Concurrent;
using System.Text;

namespace Transfermarkt.QA.Framework.Helpers;

public sealed record HttpFailure(string Method, string Url, int Status);

/// <summary>
/// Pure, thread-safe collector of HTTP 500 responses. When a host list is given, only responses
/// from exactly those hosts are recorded (compared as-is, no www stripping or subdomain matching),
/// so third-party ads and trackers cannot fail a test. Browser wiring lives in
/// <see cref="Core.HttpErrorMonitor"/>, which keeps this class trivially unit-testable.
/// </summary>
public sealed class HttpErrorLog
{
    private readonly ConcurrentQueue<HttpFailure> _failures = new();
    private readonly HashSet<string>? _monitoredHosts;

    public IReadOnlyList<HttpFailure> Failures => _failures.ToArray();

    /// <param name="monitoredHosts">Hosts to monitor, e.g. "www.transfermarkt.com". Null monitors every host.</param>
    public HttpErrorLog(IEnumerable<string>? monitoredHosts = null) =>
        _monitoredHosts = monitoredHosts is null
            ? null
            : new HashSet<string>(monitoredHosts, StringComparer.OrdinalIgnoreCase);

    private bool IsMonitored(string url) =>
        _monitoredHosts is null
        || (Uri.TryCreate(url, UriKind.Absolute, out var uri) && _monitoredHosts.Contains(uri.Host));

    public void Record(string method, string url, int status)
    {
        if (status != 500 || !IsMonitored(url)) return;

        _failures.Enqueue(new HttpFailure(method, url, status));
    }

    public string Describe()
    {
        var items = Failures;
        var sb = new StringBuilder();
        sb.AppendLine($"HTTP 500: {items.Count}");
        foreach (var f in items) sb.AppendLine($"  {f.Status} {f.Method} {f.Url}");
        return sb.ToString();
    }
}
