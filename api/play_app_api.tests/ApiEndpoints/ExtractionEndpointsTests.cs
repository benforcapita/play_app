using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using play_app_api;
using play_app_api.Data;

namespace play_app_api.tests.ApiEndpoints;

public class ExtractionEndpointsSimpleTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ExtractionEndpointsSimpleTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDb>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add test database
                services.AddDbContext<AppDb>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                });
            });
        });
    }

    [Fact]
    public async Task ExtractCharacters_ValidImageFile_ReturnsJobToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create a simple 1x1 PNG image as test data
        var imageBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==");
        
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "test-character.png");
        
        // Act
        var response = await client.PostAsync("/api/extract/characters", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(responseData.TryGetProperty("jobToken", out var jobTokenElement));
        Assert.True(responseData.TryGetProperty("message", out var messageElement));
        
        var jobToken = jobTokenElement.GetString();
        Assert.NotNull(jobToken);
        Assert.NotEmpty(jobToken);
        Assert.Equal(16, jobToken.Length); // Should be 16 characters
        
        var message = messageElement.GetString();
        Assert.Contains("Extraction job started", message);
    }

    [Fact]
    public async Task ExtractCharacters_ValidPdfFile_ReturnsJobToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create a minimal PDF file
        var pdfBytes = Encoding.UTF8.GetBytes("%PDF-1.4\n1 0 obj\n<<\n/Type /Catalog\n/Pages 2 0 R\n>>\nendobj\n%%EOF");
        
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(pdfBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "test-character.pdf");
        
        // Act
        var response = await client.PostAsync("/api/extract/characters", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(responseData.TryGetProperty("jobToken", out var jobTokenElement));
        var jobToken = jobTokenElement.GetString();
        Assert.NotNull(jobToken);
        Assert.NotEmpty(jobToken);
    }

    [Fact]
    public async Task ExtractCharacters_UnsupportedFileType_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var textBytes = Encoding.UTF8.GetBytes("This is a text file");
        
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(textBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "test.txt");
        
        // Act
        var response = await client.PostAsync("/api/extract/characters", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Unsupported content type", responseContent);
    }

    [Fact]
    public async Task ExtractCharacters_NonMultipartRequest_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var content = new StringContent("test", Encoding.UTF8, "application/json");
        
        // Act
        var response = await client.PostAsync("/api/extract/characters", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("multipart/form-data expected", responseContent);
    }

    [Fact]
    public async Task GetJobStatus_NonExistentJob_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentJobToken = "nonexistent123";
        
        // Act
        var response = await client.GetAsync($"/api/extract/jobs/{nonExistentJobToken}/status");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(responseData.TryGetProperty("message", out var message));
        Assert.Equal("Job not found", message.GetString());
    }

    [Fact]
    public async Task GetJobResult_NonExistentJob_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var nonExistentJobToken = "nonexistent456";
        
        // Act
        var response = await client.GetAsync($"/api/extract/jobs/{nonExistentJobToken}/result");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(responseData.TryGetProperty("message", out var message));
        Assert.Equal("Job not found", message.GetString());
    }

    [Theory]
    [InlineData("image/png")]
    [InlineData("image/jpeg")]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    [InlineData("application/pdf")]
    public async Task ExtractCharacters_AllSupportedContentTypes_ReturnsJobToken(string contentType)
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Create test data based on content type
        byte[] fileBytes;
        string fileName;
        
        if (contentType == "application/pdf")
        {
            fileBytes = Encoding.UTF8.GetBytes("%PDF-1.4\n1 0 obj\n<<\n/Type /Catalog\n/Pages 2 0 R\n>>\nendobj\n%%EOF");
            fileName = "test.pdf";
        }
        else
        {
            // Use the same minimal PNG for all image types (content type validation is what matters)
            fileBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==");
            fileName = contentType switch
            {
                "image/png" => "test.png",
                "image/jpeg" => "test.jpg",
                "image/webp" => "test.webp",
                "image/gif" => "test.gif",
                _ => "test.img"
            };
        }
        
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(fileBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);
        
        // Act
        var response = await client.PostAsync("/api/extract/characters", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
        
        Assert.True(responseData.TryGetProperty("jobToken", out var jobTokenElement));
        var jobToken = jobTokenElement.GetString();
        Assert.NotNull(jobToken);
        Assert.NotEmpty(jobToken);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOK()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/health");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"OK\"", content); // Health endpoint returns JSON-quoted string
    }

    [Fact]
    public async Task PingEndpoint_ReturnsValidJson()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("/ping");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(json.TryGetProperty("ok", out var ok));
        Assert.True(ok.GetBoolean());
        
        Assert.True(json.TryGetProperty("at", out var at));
        Assert.True(DateTime.TryParse(at.GetString(), out _));
    }
}