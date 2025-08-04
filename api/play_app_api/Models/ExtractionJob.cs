using System.ComponentModel.DataAnnotations;

namespace play_app_api;

public class ExtractionJob
{
    public int Id { get; set; }
    
    [Required]
    public string JobToken { get; set; } = "";
    
    [Required]
    public string FileName { get; set; } = "";
    
    [Required] 
    public string ContentType { get; set; } = "";
    
    [Required]
    public string FileDataUrl { get; set; } = "";
    
    public JobStatus Status { get; set; } = JobStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public List<SectionResult> SectionResults { get; set; } = new();
    public Character? ResultCharacter { get; set; }
    public int? ResultCharacterId { get; set; }
    
    // Computed property for job success
    public bool IsSuccessful => 
        Status == JobStatus.Completed && 
        SectionResults.Count > 0 && 
        SectionResults.Count(s => s.IsSuccessful) > (SectionResults.Count * 0.5);
}

public enum JobStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}