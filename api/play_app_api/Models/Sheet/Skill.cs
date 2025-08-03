using System.Text.Json.Serialization;

namespace play_app_api;

public class Skill
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("ability")] public string Ability { get; set; } = ""; // DEX, INT, ...
    [JsonPropertyName("proficient")] public bool Proficient { get; set; }
    [JsonPropertyName("expert")] public bool Expert { get; set; }
} 