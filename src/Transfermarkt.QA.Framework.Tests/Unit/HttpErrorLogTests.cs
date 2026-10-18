using Transfermarkt.QA.Framework.Helpers;

namespace Transfermarkt.QA.Framework.Tests.Unit;

[TestFixture, Category("Unit")]
public class HttpErrorLogTests
{
    [Test]
    public void Record_WithoutBaseUrl_Captures500FromAnyHost()
    {
        var log = new HttpErrorLog();

        log.Record("GET", "https://www.transfermarkt.com/a", 500);
        log.Record("GET", "https://tracker.example.com/b", 500);

        Assert.That(log.Failures.Select(f => f.Url), Is.EqualTo(new[]
        {
            "https://www.transfermarkt.com/a",
            "https://tracker.example.com/b"
        }));
    }

    [Test]
    public void Record_WithMonitoredHosts_CapturesOnlyThoseHostsExactly()
    {
        var log = new HttpErrorLog(["www.transfermarkt.com", "tmapi.transfermarkt.technology"]);

        log.Record("GET", "https://www.transfermarkt.com/a", 500);
        log.Record("GET", "https://TMAPI.transfermarkt.technology/b", 500);
        log.Record("GET", "https://transfermarkt.com/c", 500);
        log.Record("GET", "https://sub.www.transfermarkt.com/d", 500);
        log.Record("GET", "https://tracker.example.com/e", 500);

        Assert.That(log.Failures.Select(f => f.Url), Is.EqualTo(new[]
        {
            "https://www.transfermarkt.com/a",
            "https://TMAPI.transfermarkt.technology/b"
        }));
    }

    [TestCase(200)]
    [TestCase(404)]
    [TestCase(501)]
    [TestCase(503)]
    public void Record_IgnoresNon500Statuses(int status)
    {
        var log = new HttpErrorLog();

        log.Record("GET", "https://www.transfermarkt.com/a", status);

        Assert.That(log.Failures, Is.Empty);
    }

    [Test]
    public void Describe_ListsRecordedFailures()
    {
        var log = new HttpErrorLog();
        log.Record("POST", "https://www.transfermarkt.com/login", 500);

        Assert.That(log.Describe(), Does.Contain("500 POST https://www.transfermarkt.com/login"));
    }
}
