using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using play_app_api;
using play_app_api.Data;
using play_app_api.Services;

namespace play_app_api.tests.Services;

public class ExtractionJobServiceSimpleTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly AppDb _dbContext;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<ExtractionJobService>> _loggerMock;

    public ExtractionJobServiceSimpleTests()
    {
        var services = new ServiceCollection();
        
        // Add in-memory database
        services.AddDbContext<AppDb>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        // Add configuration
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["OPENROUTER_MODEL"]).Returns("test-model");
        services.AddSingleton(configurationMock.Object);
        
        // Setup HTTP client mocking
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        services.AddSingleton(_httpClientFactoryMock.Object);
        
        // Add logger mock
        _loggerMock = new Mock<ILogger<ExtractionJobService>>();
        services.AddSingleton(_loggerMock.Object);
        
        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<AppDb>();
    }

    [Fact]
    public void Service_CanBeConstructed_WithValidParameters()
    {
        // Arrange & Act
        var service = new ExtractionJobService(_serviceProvider, _loggerMock.Object);
        
        // Assert
        Assert.NotNull(service);
        
        // Verify logger was used in construction
        _loggerMock.Verify(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), 
            Times.Never); // Should not log during construction
    }

    [Fact]
    public async Task DatabaseOperations_Work_WithExtractionJobs()
    {
        // Arrange
        var job = new ExtractionJob
        {
            JobToken = "test123",
            FileName = "test.png",
            ContentType = "image/png",
            FileDataUrl = "data:image/png;base64,test",
            Status = JobStatus.Pending
        };

        // Act
        _dbContext.ExtractionJobs.Add(job);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedJob = await _dbContext.ExtractionJobs.FirstAsync(j => j.JobToken == "test123");
        Assert.Equal("test.png", savedJob.FileName);
        Assert.Equal(JobStatus.Pending, savedJob.Status);
    }

    [Fact]
    public async Task DatabaseOperations_Work_WithSectionResults()
    {
        // Arrange
        var job = new ExtractionJob
        {
            JobToken = "test456",
            FileName = "test.png",
            ContentType = "image/png",
            FileDataUrl = "data:image/png;base64,test",
            Status = JobStatus.Pending
        };
        
        _dbContext.ExtractionJobs.Add(job);
        await _dbContext.SaveChangesAsync();

        var sectionResult = new SectionResult
        {
            SectionName = "CharacterInfo",
            IsSuccessful = true,
            ExtractionJobId = job.Id
        };

        // Act
        _dbContext.SectionResults.Add(sectionResult);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedJob = await _dbContext.ExtractionJobs
            .Include(j => j.SectionResults)
            .FirstAsync(j => j.Id == job.Id);
            
        Assert.Single(savedJob.SectionResults);
        Assert.Equal("CharacterInfo", savedJob.SectionResults.First().SectionName);
        Assert.True(savedJob.SectionResults.First().IsSuccessful);
    }

    [Fact]
    public void Service_CanBeConstructed()
    {
        // Act & Assert - Should not throw
        var service = new ExtractionJobService(_serviceProvider, _loggerMock.Object);
        Assert.NotNull(service);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        _serviceProvider?.Dispose();
    }
}