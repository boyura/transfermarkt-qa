using System.Text.Json;

namespace Transfermarkt.QA.UiTests.Mocks;

public enum Trend { None, Up, Down }

/// <summary>
/// Intercepts the backend call that feeds the Premier League table widget, keeps the real
/// response and overrides the <c>trend</c> of the first clubs. Clubs come from the live response,
/// so no standings are hard-coded. Register before opening the page.
/// </summary>
public sealed class PremierLeagueTableApiMock(IPage page)
{
    private const string DataUrlPattern = "**/ceapi/competitionTables/index/112*";

    private readonly Dictionary<Trend, string> _clubByTrend = new();

    /// <summary>Name of the (first) club that was given this trend. Valid once the table has loaded.</summary>
    public string ClubWith(Trend trend) => _clubByTrend[trend];

    /// <summary>Row <c>i</c> of the response gets <c>trends[i]</c>.</summary>
    public async Task SetTrendsAsync(params Trend[] trends) =>
        await page.RouteAsync(DataUrlPattern, async route =>
        {
            var response = await route.FetchAsync();
            var dto = JsonSerializer.Deserialize<CompetitionTablesDto>(await response.TextAsync())!;
            var rows = dto.Tables[0].Rows;

            _clubByTrend.Clear();
            for (var i = 0; i < trends.Length; i++)
            {
                rows[i].Trend = trends[i] switch { Trend.Up => "up", Trend.Down => "down", _ => "" };
                _clubByTrend.TryAdd(trends[i], rows[i].Team.Name);
            }

            await route.FulfillAsync(new() { Response = response, Body = JsonSerializer.Serialize(dto) });
        });
}
