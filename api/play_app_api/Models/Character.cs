namespace play_app_api;
using System.ComponentModel.DataAnnotations.Schema;

public class Character
{
    public int Id { get; set; } = 0;

    public string Name { get; set; } = "";
    
    public string Class { get; set; } = "";

    public string Species { get; set; } = "";

    // Removed foreign key to CharacterSheet to avoid circular reference
    public CharacterSheet Sheet { get; set; } = new();

}