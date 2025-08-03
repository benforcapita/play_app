using System.Text.Json.Serialization;

namespace play_app_api;

public class SavingThrows
{
    [JsonPropertyName("strength")]     public ProficiencyFlag Strength { get; set; } = new();
    [JsonPropertyName("dexterity")]    public ProficiencyFlag Dexterity { get; set; } = new();
    [JsonPropertyName("constitution")] public ProficiencyFlag Constitution { get; set; } = new();
    [JsonPropertyName("intelligence")] public ProficiencyFlag Intelligence { get; set; } = new();
    [JsonPropertyName("wisdom")]       public ProficiencyFlag Wisdom { get; set; } = new();
    [JsonPropertyName("charisma")]     public ProficiencyFlag Charisma { get; set; } = new();
}

public class ProficiencyFlag 
{ 
    [JsonPropertyName("proficient")] public bool Proficient { get; set; } 
} 