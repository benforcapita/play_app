using System.Text.Json.Serialization;

namespace play_app_api;

public class Proficiencies
{
    [JsonPropertyName("armor")]     public List<string> Armor { get; set; } = new();
    [JsonPropertyName("weapons")]   public List<string> Weapons { get; set; } = new();
    [JsonPropertyName("tools")]     public List<string> Tools { get; set; } = new();
    [JsonPropertyName("languages")] public List<string> Languages { get; set; } = new();
} 