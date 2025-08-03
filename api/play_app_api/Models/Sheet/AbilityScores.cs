using System.Text.Json.Serialization;

namespace play_app_api;

public class AbilityScores
{
    [JsonPropertyName("strength")]     public AbilityScore Strength { get; set; } = new();
    [JsonPropertyName("dexterity")]    public AbilityScore Dexterity { get; set; } = new();
    [JsonPropertyName("constitution")] public AbilityScore Constitution { get; set; } = new();
    [JsonPropertyName("intelligence")] public AbilityScore Intelligence { get; set; } = new();
    [JsonPropertyName("wisdom")]       public AbilityScore Wisdom { get; set; } = new();
    [JsonPropertyName("charisma")]     public AbilityScore Charisma { get; set; } = new();
}

public class AbilityScore
{
    [JsonPropertyName("score")]    public int Score { get; set; } = 10;
    [JsonPropertyName("modifier")] public int Modifier { get; set; } = 0;
} 