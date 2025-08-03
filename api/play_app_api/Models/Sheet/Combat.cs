using System.Text.Json.Serialization;

namespace play_app_api;

public class Combat
{
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
    [JsonPropertyName("max")] public int Max { get; set; } = 10; 
    [JsonPropertyName("current")] public int Current { get; set; } = 10; 
    [JsonPropertyName("temporary")] public int Temporary { get; set; } 
}

public class HitDice 
{ 
    [JsonPropertyName("total")] public string Total { get; set; } = ""; 
    [JsonPropertyName("current")] public string Current { get; set; } = ""; 
}

public class DeathSaves 
{ 
    [JsonPropertyName("successes")] public int Successes { get; set; }; 
    [JsonPropertyName("failures")] public int Failures { get; set; } 
}

public class PassiveScores 
{ 
    [JsonPropertyName("perception")] public int Perception { get; set; } = 10; 
    [JsonPropertyName("insight")] public int Insight { get; set; } = 10; 
    [JsonPropertyName("investigation")] public int Investigation { get; set; } = 10; 
} 