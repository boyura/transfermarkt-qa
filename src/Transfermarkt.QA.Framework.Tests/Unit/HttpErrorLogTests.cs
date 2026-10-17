using Transfermarkt.QA.Tests.Helpers;

namespace Transfermarkt.QA.Tests.Unit;

[TestFixture, Category("Unit")]
public class HttpErrorLogTests
{
    [Test]
    public void Record_Captures500RegardlessOfHost()
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
