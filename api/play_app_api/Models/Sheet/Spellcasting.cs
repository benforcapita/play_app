using System.Text.Json.Serialization;

namespace play_app_api;

public class Spellcasting
{
    [JsonPropertyName("class")] public string Class { get; set; } = "";
    [JsonPropertyName("ability")] public string Ability { get; set; } = "";
    [JsonPropertyName("saveDC")] public int SaveDC { get; set; }
    [JsonPropertyName("attackBonus")] public int AttackBonus { get; set; }
    [JsonPropertyName("spellSlots")] public SpellSlots SpellSlots { get; set; } = new();
    [JsonPropertyName("cantrips")] public List<Spell> Cantrips { get; set; } = new();
    [JsonPropertyName("spellsKnown")] public List<Spell> SpellsKnown { get; set; } = new();
}

public class SpellSlots
{
    [JsonPropertyName("level1")] public Slot Level1 { get; set; } = new();
    [JsonPropertyName("level2")] public Slot Level2 { get; set; } = new();
    [JsonPropertyName("level3")] public Slot Level3 { get; set; } = new();
    [JsonPropertyName("level4")] public Slot Level4 { get; set; } = new();
    [JsonPropertyName("level5")] public Slot Level5 { get; set; } = new();
    [JsonPropertyName("level6")] public Slot Level6 { get; set; } = new();
    [JsonPropertyName("level7")] public Slot Level7 { get; set; } = new();
    [JsonPropertyName("level8")] public Slot Level8 { get; set; } = new();
    [JsonPropertyName("level9")] public Slot Level9 { get; set; } = new();
}

public class Slot 
{ 
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("used")] public int Used { get; set; } 
}

public class Spell
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("level")] public int Level { get; set; }
    [JsonPropertyName("source")] public string Source { get; set; } = "";
    [JsonPropertyName("castingTime")] public string CastingTime { get; set; } = "";
    [JsonPropertyName("range")] public string Range { get; set; } = "";
    [JsonPropertyName("components")] public string Components { get; set; } = "V, S, M";
    [JsonPropertyName("duration")] public string Duration { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("attack")] public SpellAttack Attack { get; set; } = new();
    [JsonPropertyName("save")] public SpellSave Save { get; set; } = new();
}

public class SpellAttack 
{ 
    [JsonPropertyName("bonus")] public int Bonus { get; set; }
    [JsonPropertyName("damage")] public string Damage { get; set; } = ""; 
}

public class SpellSave 
{ 
    [JsonPropertyName("ability")] public string Ability { get; set; } = ""; 
    [JsonPropertyName("dc")] public int Dc { get; set; } 
} 