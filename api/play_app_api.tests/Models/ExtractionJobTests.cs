using play_app_api;

namespace play_app_api.tests.Models;

public class ExtractionJobTests
{
    [Fact]
    public void IsSuccessful_WhenCompleted_And_MoreThan50PercentSuccess_ReturnsTrue()
    {
        // Arrange
        var job = new ExtractionJob
        {
            Status = JobStatus.Completed,
            SectionResults = new List<SectionResult>
            {
                new() { SectionName = "CharacterInfo", IsSuccessful = true },
                new() { SectionName = "Appearance", IsSuccessful = true },
                new() { SectionName = "AbilityScores", IsSuccessful = true },
                new() { SectionName = "Combat", IsSuccessful = false },
                new() { SectionName = "Skills", IsSuccessful = false }
            }
        };

        // Act & Assert - 3 out of 5 (60%) successful
        Assert.True(job.IsSuccessful);
    }

    [Fact]
    public void IsSuccessful_WhenCompleted_And_Exactly50PercentSuccess_ReturnsFalse()
    {
        // Arrange
        var job = new ExtractionJob
        {
            Status = JobStatus.Completed,
            SectionResults = new List<SectionResult>
            {
                new() { SectionName = "CharacterInfo", IsSuccessful = true },
                new() { SectionName = "Appearance", IsSuccessful = true },
                new() { SectionName = "AbilityScores", IsSuccessful = false },
                new() { SectionName = "Combat", IsSuccessful = false }
            }
        };

        // Act & Assert - exactly 2 out of 4 (50%) successful
        Assert.False(job.IsSuccessful);
    }

    [Fact]
    public void IsSuccessful_WhenCompleted_And_LessThan50PercentSuccess_ReturnsFalse()
    {
        // Arrange
        var job = new ExtractionJob
        {
            Status = JobStatus.Completed,
            SectionResults = new List<SectionResult>
            {
                new() { SectionName = "CharacterInfo", IsSuccessful = true },
                new() { SectionName = "Appearance", IsSuccessful = false },
                new() { SectionName = "AbilityScores", IsSuccessful = false },
                new() { SectionName = "Combat", IsSuccessful = false }
            }
        };

        // Act & Assert - 1 out of 4 (25%) successful
        Assert.False(job.IsSuccessful);
    }

    [Fact]
    public void IsSuccessful_WhenNotCompleted_ReturnsFalse()
    {
        // Arrange
        var job = new ExtractionJob
        {
            Status = JobStatus.InProgress,
            SectionResults = new List<SectionResult>
            {
                new() { SectionName = "CharacterInfo", IsSuccessful = true },
                new() { SectionName = "Appearance", IsSuccessful = true },
                new() { SectionName = "AbilityScores", IsSuccessful = true }
            }
        };

        // Act & Assert - Even with 100% success, if not completed, returns false
        Assert.False(job.IsSuccessful);
    }

    [Fact]
    public void IsSuccessful_WhenFailed_ReturnsFalse()
    {
        // Arrange
        var job = new ExtractionJob
        {
            Status = JobStatus.Failed,
            SectionResults = new List<SectionResult>
            {
                new() { SectionName = "CharacterInfo", IsSuccessful = true },
                new() { SectionName = "Appearance", IsSuccessful = true },
                new() { SectionName = "AbilityScores", IsSuccessful = true }
            }
        };

        // Act & Assert
        Assert.False(job.IsSuccessful);
    }

    [Fact]
    public void IsSuccessful_WhenCompletedButNoSections_ReturnsFalse()
    {
        // Arrange
        var job = new ExtractionJob
        {
            Status = JobStatus.Completed,
            SectionResults = new List<SectionResult>()
        };

        // Act & Assert
        Assert.False(job.IsSuccessful);
    }

    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var job = new ExtractionJob();

        // Assert
        Assert.Equal("", job.JobToken);
        Assert.Equal("", job.FileName);
        Assert.Equal("", job.ContentType);
        Assert.Equal("", job.FileDataUrl);
        Assert.Equal(JobStatus.Pending, job.Status);
        Assert.Empty(job.SectionResults);
        Assert.Null(job.ResultCharacter);
        Assert.Null(job.ResultCharacterId);
        Assert.Null(job.ErrorMessage);
        Assert.Null(job.StartedAt);
        Assert.Null(job.CompletedAt);
        
        // CreatedAt should be set to approximately now
        Assert.True(DateTime.UtcNow.Subtract(job.CreatedAt).TotalMinutes < 1);
    }

    [Theory]
    [InlineData(JobStatus.Pending)]
    [InlineData(JobStatus.InProgress)]
    [InlineData(JobStatus.Completed)]
    [InlineData(JobStatus.Failed)]
    public void Status_CanBeSetToAllValidValues(JobStatus status)
    {
        // Arrange
        var job = new ExtractionJob();

        // Act
        job.Status = status;

        // Assert
        Assert.Equal(status, job.Status);
    }
}