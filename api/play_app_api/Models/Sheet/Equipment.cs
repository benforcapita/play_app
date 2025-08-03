using System.Text.Json.Serialization;

namespace play_app_api;

public class Equipment
{
    [JsonPropertyName("items")] public List<Item> Items { get; set; } = new();
    [JsonPropertyName("currency")] public Currency Currency { get; set; } = new();
    [JsonPropertyName("carryingCapacity")] public CarryingCapacity CarryingCapacity { get; set; } = new();
}

public class Item 
{ 
    [JsonPropertyName("name")] public string Name { get; set; } = ""; 
    [JsonPropertyName("quantity")] public int Quantity { get; set; } = 1; 
    [JsonPropertyName("weight")] public float Weight { get; set; } 
}

public class Currency 
{ 
    [JsonPropertyName("cp")] public int Cp { get; set; }
    [JsonPropertyName("sp")] public int Sp { get; set; }
    [JsonPropertyName("ep")] public int Ep { get; set; }
    [JsonPropertyName("gp")] public int Gp { get; set; }
    [JsonPropertyName("pp")] public int Pp { get; set; } 
}

public class CarryingCapacity 
{ 
    [JsonPropertyName("weightCarried")] public float WeightCarried { get; set; }
    [JsonPropertyName("encumbered")] public float Encumbered { get; set; } 
    [JsonPropertyName("pushDragLift")] public float PushDragLift { get; set; } 
} 