using System.Text.Json.Serialization;

namespace play_app_api;

public class Backstory
{
    [JsonPropertyName("alliesAndOrganizations")] public string AlliesAndOrganizations { get; set; } = "";
    [JsonPropertyName("characterBackstory")] public string CharacterBackstory { get; set; } = "";
    [JsonPropertyName("additionalNotes")] public string AdditionalNotes { get; set; } = "";
} 