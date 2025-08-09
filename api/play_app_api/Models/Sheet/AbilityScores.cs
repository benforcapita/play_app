using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace play_app_api;

public class AbilityScores
{
    public int Id { get; set; }
    public int CharacterSheetId { get; set; }
    public CharacterSheet CharacterSheet { get; set; } = null!;
    
    // Use distinct foreign key properties for each ability score
    public int StrengthId { get; set; }
    [ForeignKey("StrengthId")]
    [JsonPropertyName("strength")] 
    public AbilityScore Strength { get; set; } = new();
    
    public int DexterityId { get; set; }
    [ForeignKey("DexterityId")]
    [JsonPropertyName("dexterity")] 
    public AbilityScore Dexterity { get; set; } = new();
    
    public int ConstitutionId { get; set; }
    [ForeignKey("ConstitutionId")]
    [JsonPropertyName("constitution")] 
    public AbilityScore Constitution { get; set; } = new();
    
    public int IntelligenceId { get; set; }
    [ForeignKey("IntelligenceId")]
    [JsonPropertyName("intelligence")] 
    public AbilityScore Intelligence { get; set; } = new();
    
    public int WisdomId { get; set; }
    [ForeignKey("WisdomId")]
    [JsonPropertyName("wisdom")] 
    public AbilityScore Wisdom { get; set; } = new();
    
    public int CharismaId { get; set; }
    [ForeignKey("CharismaId")]
    [JsonPropertyName("charisma")] 
    public AbilityScore Charisma { get; set; } = new();
}

public class AbilityScore
{
    public int Id { get; set; }
    [JsonPropertyName("score")]    public int Score { get; set; } = 10;
    [JsonPropertyName("modifier")] public int Modifier { get; set; } = 0;
} 