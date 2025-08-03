using System.Text.Json.Serialization;

namespace play_app_api;

public class Appearance
{
    [JsonPropertyName("size")] public string Size { get; set; } = "";
    [JsonPropertyName("gender")] public string Gender { get; set; } = "";
    [JsonPropertyName("age")] public string Age { get; set; } = "";
    [JsonPropertyName("height")] public string Height { get; set; } = "";
    [JsonPropertyName("weight")] public string Weight { get; set; } = "";
    [JsonPropertyName("skin")] public string Skin { get; set; } = "";
    [JsonPropertyName("eyes")] public string Eyes { get; set; } = "";
    [JsonPropertyName("hair")] public string Hair { get; set; } = "";
} 