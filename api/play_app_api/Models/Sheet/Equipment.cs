using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace play_app_api;

public class Equipment
{
    public int Id { get; set; }
    public int CharacterSheetId { get; set; }
    [ForeignKey("CharacterSheetId")]
    public CharacterSheet CharacterSheet { get; set; } = null!;
    [JsonPropertyName("items")] 
    [JsonConverter(typeof(ItemsConverter))]
    public List<Item> Items { get; set; } = new();
    
    [JsonPropertyName("currency")] public Currency Currency { get; set; } = new();
    [JsonPropertyName("carryingCapacity")] public CarryingCapacity CarryingCapacity { get; set; } = new();
}

public class Item 
{ 
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    [ForeignKey("EquipmentId")]
    public Equipment Equipment { get; set; } = null!;
    [JsonPropertyName("name")] public string Name { get; set; } = ""; 
    [JsonPropertyName("quantity")] public int Quantity { get; set; } = 1; 
    [JsonPropertyName("weight")] public float Weight { get; set; } 
}

public class Currency 
{ 
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    [ForeignKey("EquipmentId")]
    public Equipment Equipment { get; set; } = null!;
    [JsonPropertyName("cp")] public int Cp { get; set; }
    [JsonPropertyName("sp")] public int Sp { get; set; }
    [JsonPropertyName("ep")] public int Ep { get; set; }
    [JsonPropertyName("gp")] public int Gp { get; set; }
    [JsonPropertyName("pp")] public int Pp { get; set; } 
}

public class CarryingCapacity 
{ 
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    [ForeignKey("EquipmentId")]
    public Equipment Equipment { get; set; } = null!;
    [JsonPropertyName("weightCarried")] public float WeightCarried { get; set; }
    [JsonPropertyName("encumbered")] public float Encumbered { get; set; } 
    [JsonPropertyName("pushDragLift")] public float PushDragLift { get; set; } 
}

// Custom converter to handle both string arrays and object arrays
public class ItemsConverter : JsonConverter<List<Item>>
{
    public override List<Item> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var items = new List<Item>();
        
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            reader.Read();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    // Handle string format like "Dagger (3)"
                    var itemString = reader.GetString() ?? "";
                    var item = ParseItemFromString(itemString);
                    items.Add(item);
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    // Handle object format
                    var item = JsonSerializer.Deserialize<Item>(ref reader, options);
                    if (item != null)
                        items.Add(item);
                }
                reader.Read();
            }
        }
        
        return items;
    }
    
    private Item ParseItemFromString(string itemString)
    {
        var item = new Item { Name = itemString, Quantity = 1, Weight = 0 };
        
        // Try to extract quantity from format like "Dagger (3)"
        var match = System.Text.RegularExpressions.Regex.Match(itemString, @"(.+?)\s*\((\d+)\)");
        if (match.Success)
        {
            item.Name = match.Groups[1].Value.Trim();
            if (int.TryParse(match.Groups[2].Value, out int quantity))
                item.Quantity = quantity;
        }
        
        return item;
    }
    
    public override void Write(Utf8JsonWriter writer, List<Item> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
} 