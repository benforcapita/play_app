using System.ComponentModel.DataAnnotations;

namespace play_app_api;

public class SectionResult
{
    public int Id { get; set; }
    
    [Required]
    public string SectionName { get; set; } = "";
    
    public bool IsSuccessful { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key
    public int ExtractionJobId { get; set; }
    public ExtractionJob ExtractionJob { get; set; } = null!;
}