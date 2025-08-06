using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace play_app_api;

public class Combat
{
    public int Id { get; set; }
    public int CharacterSheetId { get; set; }
    [ForeignKey("CharacterSheetId")]
    public CharacterSheet CharacterSheet { get; set; } = null!;
    [JsonPropertyName("armorClass")] public int ArmorClass { get; set; } = 10;
    [JsonPropertyName("initiative")] public int Initiative { get; set; } = 0;
    [JsonPropertyName("speed")] public string Speed { get; set; } = "30 ft.";
    [JsonPropertyName("proficiencyBonus")] public int ProficiencyBonus { get; set; } = 2;
    [JsonPropertyName("inspiration")] public bool Inspiration { get; set; }
    [JsonPropertyName("hitPoints")] public HitPoints HitPoints { get; set; } = new();
    [JsonPropertyName("hitDice")] public HitDice HitDice { get; set; } = new();
    [JsonPropertyName("deathSaves")] public DeathSaves DeathSaves { get; set; } = new();
    [JsonPropertyName("passiveScores")] public PassiveScores PassiveScores { get; set; } = new();
}

public class HitPoints 
{ 
    public int Id { get; set; }
    public int CombatId { get; set; }
    [ForeignKey("CombatId")]
    public Combat Combat { get; set; } = null!;
    [JsonPropertyName("max")] public int Max { get; set; } = 10; 
    [JsonPropertyName("current")] public int Current { get; set; } = 10; 
    [JsonPropertyName("temporary")] public int Temporary { get; set; } 
}

public class HitDice 
{ 
    public int Id { get; set; }
    public int CombatId { get; set; }
    [ForeignKey("CombatId")]
    public Combat Combat { get; set; } = null!;
    [JsonPropertyName("total")] public string Total { get; set; } = ""; 
    [JsonPropertyName("current")] public string Current { get; set; } = ""; 
}

public class DeathSaves 
{ 
    public int Id { get; set; }
    public int CombatId { get; set; }
    [ForeignKey("CombatId")]
    public Combat Combat { get; set; } = null!;
    [JsonPropertyName("successes")] public int Successes { get; set; }
    [JsonPropertyName("failures")] public int Failures { get; set; } 
}

public class PassiveScores 
{ 
    public int Id { get; set; }
    public int CombatId { get; set; }
    [ForeignKey("CombatId")]
    public Combat Combat { get; set; } = null!;
    [JsonPropertyName("perception")] public int Perception { get; set; } = 10; 
    [JsonPropertyName("insight")] public int Insight { get; set; } = 10; 
    [JsonPropertyName("investigation")] public int Investigation { get; set; } = 10; 
} 