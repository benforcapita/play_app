using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace play_app_api;

public class SavingThrows
{
    public int Id { get; set; }
    public int CharacterSheetId { get; set; }
    [ForeignKey("CharacterSheetId")]
    public CharacterSheet CharacterSheet { get; set; } = null!;
    
    // Use distinct foreign key properties for each saving throw
    public int StrengthSaveId { get; set; }
    [ForeignKey("StrengthSaveId")]
    [JsonPropertyName("strength")] 
    public SavingThrow Strength { get; set; } = new();
    
    public int DexteritySaveId { get; set; }
    [ForeignKey("DexteritySaveId")]
    [JsonPropertyName("dexterity")] 
    public SavingThrow Dexterity { get; set; } = new();
    
    public int ConstitutionSaveId { get; set; }
    [ForeignKey("ConstitutionSaveId")]
    [JsonPropertyName("constitution")] 
    public SavingThrow Constitution { get; set; } = new();
    
    public int IntelligenceSaveId { get; set; }
    [ForeignKey("IntelligenceSaveId")]
    [JsonPropertyName("intelligence")] 
    public SavingThrow Intelligence { get; set; } = new();
    
    public int WisdomSaveId { get; set; }
    [ForeignKey("WisdomSaveId")]
    [JsonPropertyName("wisdom")] 
    public SavingThrow Wisdom { get; set; } = new();
    
    public int CharismaSaveId { get; set; }
    [ForeignKey("CharismaSaveId")]
    [JsonPropertyName("charisma")] 
    public SavingThrow Charisma { get; set; } = new();
}

public class SavingThrow 
{ 
    public int Id { get; set; }
    [JsonPropertyName("proficient")] public bool Proficient { get; set; } = false;
} 