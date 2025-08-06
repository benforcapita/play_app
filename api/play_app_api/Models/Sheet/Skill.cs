using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace play_app_api;

public class Skill
{
    public int Id { get; set; }
    public int CharacterSheetId { get; set; }
    [ForeignKey("CharacterSheetId")]
    public CharacterSheet CharacterSheet { get; set; } = null!;
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("ability")] public string Ability { get; set; } = "";
    [JsonPropertyName("modifier")] public int Modifier { get; set; } = 0;
    [JsonPropertyName("proficient")] public bool Proficient { get; set; } = false;
    [JsonPropertyName("expertise")] public bool Expertise { get; set; } = false;
} 