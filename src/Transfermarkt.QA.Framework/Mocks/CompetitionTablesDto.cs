using System.Text.Json;
using System.Text.Json.Serialization;

namespace Transfermarkt.QA.Tests.Mocks;

// Shape of /ceapi/competitionTables/index/{id}. Only the fields tests touch are typed; everything
// else is kept in [JsonExtensionData] so the response is sent back to the page unchanged.

public sealed class CompetitionTablesDto
{
    [JsonPropertyName("tables")] public List<TableDto> Tables { get; set; } = [];
    [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

public sealed class TableDto
{
    [JsonPropertyName("rows")] public List<RowDto> Rows { get; set; } = [];
    [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

public sealed class RowDto
{
    [JsonPropertyName("team")] public TeamDto Team { get; set; } = new();

    /// <summary>"" (no trend), "up" or "down".</summary>
    [JsonPropertyName("trend")] public string Trend { get; set; } = "";

    [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

public sealed class TeamDto
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}
