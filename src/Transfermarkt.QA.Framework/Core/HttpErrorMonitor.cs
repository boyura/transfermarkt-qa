using Transfermarkt.QA.Framework.Config;
using Transfermarkt.QA.Framework.Helpers;

namespace Transfermarkt.QA.Framework.Core;

/// <summary>
/// Subscribes to every response in a BrowserContext (incl. popups and iframes) and feeds
/// the pure <see cref="HttpErrorLog"/>. Event-driven: no polling or sleeps.
/// </summary>
public sealed class HttpErrorMonitor : IDisposable
{
    private readonly IBrowserContext _context;

    public HttpErrorLog Log { get; }

    public HttpErrorMonitor(IBrowserContext context)
    {
        _context = context;
        Log = new HttpErrorLog(TestSettings.MonitoredHosts);
        _context.Response += OnResponse;
    }

    private void OnResponse(object? sender, IResponse response) =>
        Log.Record(response.Request.Method, response.Url, response.Status);

    public void Dispose() => _context.Response -= OnResponse;
}
