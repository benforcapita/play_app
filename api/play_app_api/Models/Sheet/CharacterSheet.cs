using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace play_app_api;

// ---------- ROOT ----------
public class CharacterSheet
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    [ForeignKey("CharacterId")]
    [JsonIgnore]
    public Character Character { get; set; } = null!;
    
    [JsonPropertyName("characterInfo")]     public CharacterInfo CharacterInfo { get; set; } = new();
    [JsonPropertyName("appearance")]        public Appearance Appearance { get; set; } = new();
    [JsonPropertyName("abilityScores")]     public AbilityScores AbilityScores { get; set; } = new();
    [JsonPropertyName("savingThrows")]      public SavingThrows SavingThrows { get; set; } = new();
    [JsonPropertyName("skills")]            public List<Skill> Skills { get; set; } = new();
    [JsonPropertyName("combat")]            public Combat Combat { get; set; } = new();
    [JsonPropertyName("proficiencies")]     public Proficiencies Proficiencies { get; set; } = new();
    [JsonPropertyName("featuresAndTraits")] public List<FeatureTrait> FeaturesAndTraits { get; set; } = new();
    [JsonPropertyName("equipment")]         public Equipment Equipment { get; set; } = new();
    [JsonPropertyName("spellcasting")]      public Spellcasting Spellcasting { get; set; } = new();
    [JsonPropertyName("persona")]           public Persona Persona { get; set; } = new();
    [JsonPropertyName("backstory")]         public Backstory Backstory { get; set; } = new();
}
