using System.Text.Json.Serialization;

namespace play_app_api;

public class CharacterInfo
{
    [JsonPropertyName("characterName")]     public string CharacterName { get; set; } = "";
    [JsonPropertyName("playerName")]        public string PlayerName { get; set; } = "";
    [JsonPropertyName("classAndLevel")]     public string ClassAndLevel { get; set; } = "";
    [JsonPropertyName("species")]           public string Species { get; set; } = "";
    [JsonPropertyName("background")]        public string Background { get; set; } = "";
    [JsonPropertyName("experiencePoints")]  public string ExperiencePoints { get; set; } = "";
    [JsonPropertyName("alignment")]         public string Alignment { get; set; } = "";
} 