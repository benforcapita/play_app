using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace play_app_api;

public class Spellcasting
{
    public int Id { get; set; }
    public int CharacterSheetId { get; set; }
    [JsonIgnore]
    public CharacterSheet CharacterSheet { get; set; } = null!;
    [JsonPropertyName("class")] public string Class { get; set; } = "";
    [JsonPropertyName("ability")] public string Ability { get; set; } = "";
    [JsonPropertyName("saveDC")] public int SaveDC { get; set; }
    [JsonPropertyName("attackBonus")] public int AttackBonus { get; set; }
    
    public int SpellSlotsId { get; set; }
    [ForeignKey("SpellSlotsId")]
    [JsonPropertyName("spellSlots")] 
    public SpellSlots SpellSlots { get; set; } = new();
    
    [JsonPropertyName("cantrips")] public List<Spell> Cantrips { get; set; } = new();
    [JsonPropertyName("spellsKnown")] public List<Spell> SpellsKnown { get; set; } = new();
}

public class SpellSlots
{
    public int Id { get; set; }
    
    // Use distinct foreign key properties for each spell slot level
    public int Level1Id { get; set; }
    [ForeignKey("Level1Id")]
    [JsonPropertyName("level1")] 
    public Slot Level1 { get; set; } = new();
    
    public int Level2Id { get; set; }
    [ForeignKey("Level2Id")]
    [JsonPropertyName("level2")] 
    public Slot Level2 { get; set; } = new();
    
    public int Level3Id { get; set; }
    [ForeignKey("Level3Id")]
    [JsonPropertyName("level3")] 
    public Slot Level3 { get; set; } = new();
    
    public int Level4Id { get; set; }
    [ForeignKey("Level4Id")]
    [JsonPropertyName("level4")] 
    public Slot Level4 { get; set; } = new();
    
    public int Level5Id { get; set; }
    [ForeignKey("Level5Id")]
    [JsonPropertyName("level5")] 
    public Slot Level5 { get; set; } = new();
    
    public int Level6Id { get; set; }
    [ForeignKey("Level6Id")]
    [JsonPropertyName("level6")] 
    public Slot Level6 { get; set; } = new();
    
    public int Level7Id { get; set; }
    [ForeignKey("Level7Id")]
    [JsonPropertyName("level7")] 
    public Slot Level7 { get; set; } = new();
    
    public int Level8Id { get; set; }
    [ForeignKey("Level8Id")]
    [JsonPropertyName("level8")] 
    public Slot Level8 { get; set; } = new();
    
    public int Level9Id { get; set; }
    [ForeignKey("Level9Id")]
    [JsonPropertyName("level9")] 
    public Slot Level9 { get; set; } = new();
}

public class Slot 
{ 
    public int Id { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("used")] public int Used { get; set; } 
}

public class Spell
{
    public int Id { get; set; }
    
    // Separate foreign keys for cantrips and spells known
    public int? CantripSpellcastingId { get; set; }
    public int? SpellsKnownSpellcastingId { get; set; }
    
    // Navigation properties
    public Spellcasting? CantripSpellcasting { get; set; }
    public Spellcasting? SpellsKnownSpellcasting { get; set; }
    
    // Add a property to distinguish between cantrips and spells known
    public string SpellType { get; set; } = ""; // "cantrip" or "spell"
    
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("level")] public int Level { get; set; }
    [JsonPropertyName("source")] public string Source { get; set; } = "";
    [JsonPropertyName("castingTime")] public string CastingTime { get; set; } = "";
    [JsonPropertyName("range")] public string Range { get; set; } = "";
    [JsonPropertyName("components")] public string Components { get; set; } = "V, S, M";
    [JsonPropertyName("duration")] public string Duration { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    
    public int AttackId { get; set; }
    [ForeignKey("AttackId")]
    [JsonPropertyName("attack")] 
    public SpellAttack Attack { get; set; } = new();
    
    public int SaveId { get; set; }
    [ForeignKey("SaveId")]
    [JsonPropertyName("save")] 
    public SpellSave Save { get; set; } = new();
}

public class SpellAttack 
{ 
    public int Id { get; set; }
    [JsonPropertyName("bonus")] public int Bonus { get; set; }
    [JsonPropertyName("damage")] public string Damage { get; set; } = ""; 
}

public class SpellSave 
{ 
    public int Id { get; set; }
    [JsonPropertyName("ability")] public string Ability { get; set; } = ""; 
    [JsonPropertyName("dc")] public int Dc { get; set; } 
} 