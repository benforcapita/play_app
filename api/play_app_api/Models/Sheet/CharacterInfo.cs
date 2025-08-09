using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace play_app_api;

public class CharacterInfo
{
    public int Id { get; set; }
    public int CharacterSheetId { get; set; }
    public CharacterSheet CharacterSheet { get; set; } = null!;
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("class")] public string Class { get; set; } = "";
    [JsonPropertyName("level")] public int Level { get; set; } = 1;
    [JsonPropertyName("species")] public string Species { get; set; } = "";
    [JsonPropertyName("subclass")] public string Subclass { get; set; } = "";
    [JsonPropertyName("alignment")] public string Alignment { get; set; } = "";
    [JsonPropertyName("experience")] public int Experience { get; set; } = 0;
    [JsonPropertyName("nextLevelXp")] public int NextLevelXp { get; set; } = 300;
    
    // Compatibility properties for ExtractionJobService
    public string CharacterName { get => Name; set => Name = value; }
    public string ClassAndLevel { get => $"{Class} {Level}"; set { /* Parsing logic if needed */ } }
} 