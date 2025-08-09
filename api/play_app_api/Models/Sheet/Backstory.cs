using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace play_app_api;

public class Backstory
{
    public int Id { get; set; }
    public int CharacterSheetId { get; set; }
    public CharacterSheet CharacterSheet { get; set; } = null!;
    [JsonPropertyName("background")] public string Background { get; set; } = "";
    [JsonPropertyName("faction")] public string Faction { get; set; } = "";
    [JsonPropertyName("origin")] public string Origin { get; set; } = "";
    [JsonPropertyName("significantOthers")] public string SignificantOthers { get; set; } = "";
    [JsonPropertyName("importantEvents")] public string ImportantEvents { get; set; } = "";
    [JsonPropertyName("alliesAndEnemies")] public string AlliesAndEnemies { get; set; } = "";
} 