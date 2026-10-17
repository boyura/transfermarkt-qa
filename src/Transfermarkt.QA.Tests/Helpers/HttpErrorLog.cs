using System.Collections.Concurrent;
using System.Text;

namespace Transfermarkt.QA.Tests.Helpers;

public sealed record HttpFailure(string Method, string Url, int Status);

/// <summary>
/// Pure, thread-safe collector of 5xx responses. Browser wiring lives in
/// <see cref="Core.HttpErrorMonitor"/>, which keeps this class trivially unit-testable.
/// First-party failures are what the application owns; third-party 5xx (ads, trackers)
/// are recorded separately so they are visible in logs but do not fail the build.
/// </summary>
public sealed class HttpErrorLog
{
    private readonly ConcurrentQueue<HttpFailure> _firstParty = new();
    private readonly ConcurrentQueue<HttpFailure> _thirdParty = new();

    public IReadOnlyList<HttpFailure> FirstPartyServerErrors => _firstParty.ToArray();
    public IReadOnlyList<HttpFailure> ThirdPartyServerErrors => _thirdParty.ToArray();

    public void Record(string method, string url, int status)
    {
        if (status < 500 || status > 599) return;

        var failure = new HttpFailure(method, url, status);
        (HostMatcher.IsFirstParty(url) ? _firstParty : _thirdParty).Enqueue(failure);
    }

    public string Describe()
    {
        var sb = new StringBuilder();
        Append(sb, "First-party 5xx", FirstPartyServerErrors);
        Append(sb, "Third-party 5xx (ignored)", ThirdPartyServerErrors);
        return sb.ToString();
    }

    private static void Append(StringBuilder sb, string title, IReadOnlyList<HttpFailure> items)
    {
        sb.AppendLine($"{title}: {items.Count}");
        foreach (var f in items) sb.AppendLine($"  {f.Status} {f.Method} {f.Url}");
    }
}
