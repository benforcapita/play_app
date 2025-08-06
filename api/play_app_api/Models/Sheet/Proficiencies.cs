using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace play_app_api;

public class Proficiencies
{
    public int Id { get; set; }
    public int CharacterSheetId { get; set; }
    [ForeignKey("CharacterSheetId")]
    public CharacterSheet CharacterSheet { get; set; } = null!;
    [JsonPropertyName("armor")]     public List<string> Armor { get; set; } = new();
    [JsonPropertyName("weapons")]   public List<string> Weapons { get; set; } = new();
    [JsonPropertyName("tools")]     public List<string> Tools { get; set; } = new();
    [JsonPropertyName("languages")] public List<string> Languages { get; set; } = new();
} 