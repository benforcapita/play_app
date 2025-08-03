using System.Text.Json.Serialization;

namespace play_app_api;

public class FeatureTrait
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("source")] public string Source { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("uses")] public string Uses { get; set; } = "";
    [JsonPropertyName("action")] public string Action { get; set; } = "";
} 