using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace play_app_api;

public class Appearance
{
    public int Id { get; set; }
    public int CharacterSheetId { get; set; }
    [ForeignKey("CharacterSheetId")]
    public CharacterSheet CharacterSheet { get; set; } = null!;
    [JsonPropertyName("size")] public string Size { get; set; } = "";
    [JsonPropertyName("gender")] public string Gender { get; set; } = "";
    [JsonPropertyName("age")] public string Age { get; set; } = "";
    [JsonPropertyName("height")] public string Height { get; set; } = "";
    [JsonPropertyName("weight")] public string Weight { get; set; } = "";
    [JsonPropertyName("skin")] public string Skin { get; set; } = "";
    [JsonPropertyName("eyes")] public string Eyes { get; set; } = "";
    [JsonPropertyName("hair")] public string Hair { get; set; } = "";
} 