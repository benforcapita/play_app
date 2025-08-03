using System.Text.Json.Serialization;

namespace play_app_api;

public class Persona
{
    [JsonPropertyName("personalityTraits")] public string PersonalityTraits { get; set; } = "";
    [JsonPropertyName("ideals")] public string Ideals { get; set; } = "";
    [JsonPropertyName("bonds")] public string Bonds { get; set; } = "";
    [JsonPropertyName("flaws")] public string Flaws { get; set; } = "";
} 