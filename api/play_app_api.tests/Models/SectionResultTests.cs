using play_app_api;

namespace play_app_api.tests.Models;

public class SectionResultTests
{
    [Fact]
    public void DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var sectionResult = new SectionResult();

        // Assert
        Assert.Equal("", sectionResult.SectionName);
        Assert.False(sectionResult.IsSuccessful);
        Assert.Null(sectionResult.ErrorMessage);
        Assert.Equal(0, sectionResult.ExtractionJobId);
        
        // ProcessedAt should be set to approximately now
        Assert.True(DateTime.UtcNow.Subtract(sectionResult.ProcessedAt).TotalMinutes < 1);
    }

    [Fact]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var sectionResult = new SectionResult();

        // Act
        sectionResult.SectionName = "CharacterInfo";
        sectionResult.IsSuccessful = true;
        sectionResult.ErrorMessage = "Test error";
        sectionResult.ExtractionJobId = 123;
        var testDate = DateTime.UtcNow.AddMinutes(-10);
        sectionResult.ProcessedAt = testDate;

        // Assert
        Assert.Equal("CharacterInfo", sectionResult.SectionName);
        Assert.True(sectionResult.IsSuccessful);
        Assert.Equal("Test error", sectionResult.ErrorMessage);
        Assert.Equal(123, sectionResult.ExtractionJobId);
        Assert.Equal(testDate, sectionResult.ProcessedAt);
    }

    [Theory]
    [InlineData("CharacterInfo")]
    [InlineData("Appearance")]
    [InlineData("AbilityScores")]
    [InlineData("SavingThrows")]
    [InlineData("Skills")]
    [InlineData("Combat")]
    [InlineData("Proficiencies")]
    [InlineData("FeaturesAndTraits")]
    [InlineData("Equipment")]
    [InlineData("Spellcasting")]
    [InlineData("Persona")]
    [InlineData("Backstory")]
    public void SectionName_AcceptsAllValidSectionNames(string sectionName)
    {
        // Arrange
        var sectionResult = new SectionResult();

        // Act
        sectionResult.SectionName = sectionName;

        // Assert
        Assert.Equal(sectionName, sectionResult.SectionName);
    }

    [Fact]
    public void IsSuccessful_CanBeSetToBothValues()
    {
        // Arrange
        var sectionResult = new SectionResult();

        // Act & Assert - Test true
        sectionResult.IsSuccessful = true;
        Assert.True(sectionResult.IsSuccessful);

        // Act & Assert - Test false
        sectionResult.IsSuccessful = false;
        Assert.False(sectionResult.IsSuccessful);
    }

    [Fact]
    public void ErrorMessage_CanBeNullOrString()
    {
        // Arrange
        var sectionResult = new SectionResult();

        // Act & Assert - Test null
        sectionResult.ErrorMessage = null;
        Assert.Null(sectionResult.ErrorMessage);

        // Act & Assert - Test string
        sectionResult.ErrorMessage = "Some error occurred";
        Assert.Equal("Some error occurred", sectionResult.ErrorMessage);

        // Act & Assert - Test empty string
        sectionResult.ErrorMessage = "";
        Assert.Equal("", sectionResult.ErrorMessage);
    }
}