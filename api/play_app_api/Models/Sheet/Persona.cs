using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace play_app_api;

public class Persona
{
    public int Id { get; set; }
    public int CharacterSheetId { get; set; }
    [JsonIgnore]
    public CharacterSheet CharacterSheet { get; set; } = null!;
    [JsonPropertyName("personalityTraits")] public string PersonalityTraits { get; set; } = "";
    [JsonPropertyName("ideals")] public string Ideals { get; set; } = "";
    [JsonPropertyName("bonds")] public string Bonds { get; set; } = "";
    [JsonPropertyName("flaws")] public string Flaws { get; set; } = "";
} 