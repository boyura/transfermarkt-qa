using System.Collections.Concurrent;
using System.Text;

namespace Transfermarkt.QA.Tests.Helpers;

public sealed record HttpFailure(string Method, string Url, int Status);

/// <summary>
/// Pure, thread-safe collector of HTTP 500 responses from any host. Browser wiring lives in
/// <see cref="Core.HttpErrorMonitor"/>, which keeps this class trivially unit-testable.
/// </summary>
public sealed class HttpErrorLog
{
    private readonly ConcurrentQueue<HttpFailure> _failures = new();

    public IReadOnlyList<HttpFailure> Failures => _failures.ToArray();

    public void Record(string method, string url, int status)
    {
        if (status != 500) return;

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
