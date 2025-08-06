namespace play_app_api;
using System.ComponentModel.DataAnnotations.Schema;

public class Character
{
    public int Id { get; set; } = 0;

    public string Name { get; set; } = "";
    
    public string Class { get; set; } = "";

    public string Species { get; set; } = "";

    public int CharacterSheetId { get; set; }
    [ForeignKey("CharacterSheetId")]
    public CharacterSheet Sheet { get; set; } = new();

}