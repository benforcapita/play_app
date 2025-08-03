using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using play_app_api;
using play_app_api.Data;
using play_app_api.ApiEndpoints;
using System.Net;
using System.Text;
using System.Text.Json;

namespace play_app_api.tests;

public class CharacterEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CharacterEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDb>));
                if (descriptor != null)
                    services.Remove(descriptor);
                services.AddDbContext<AppDb>(options =>
                    options.UseInMemoryDatabase("TestDb")
                );
            });
        });
        _client = _factory.CreateClient();
    }

    private void ClearDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDb>();
        db.Characters.RemoveRange(db.Characters);
        db.SaveChanges();
    }

    [Fact]
    public async Task GetCharacters_ShouldReturnEmptyList_WhenNoCharactersExist()
    {
        ClearDatabase();
        var response = await _client.GetAsync("/api/characters");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var characters = JsonSerializer.Deserialize<List<Character>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(characters);
        Assert.Empty(characters);
    }

    [Fact]
    public async Task GetCharacters_ShouldReturnAllCharacters_WhenCharactersExist()
    {
        ClearDatabase();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDb>();
            db.Characters.Add(new Character { Id = 1, Name = "Test1", Class = "Fighter", Species = "Human" });
            db.Characters.Add(new Character { Id = 2, Name = "Test2", Class = "Wizard", Species = "Elf" });
            db.SaveChanges();
        }
        var response = await _client.GetAsync("/api/characters");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var characters = JsonSerializer.Deserialize<List<Character>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(characters);
        Assert.Equal(2, characters.Count);
        Assert.Contains(characters, c => c.Name == "Test1" && c.Class == "Fighter");
        Assert.Contains(characters, c => c.Name == "Test2" && c.Class == "Wizard");
    }

    [Fact]
    public async Task GetCharacterById_ShouldReturnCharacter_WhenCharacterExists()
    {
        ClearDatabase();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDb>();
            db.Characters.Add(new Character { Id = 1, Name = "Test", Class = "Fighter", Species = "Human" });
            db.SaveChanges();
        }
        var response = await _client.GetAsync("/api/characters/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var returnedCharacter = JsonSerializer.Deserialize<Character>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(returnedCharacter);
        Assert.Equal(1, returnedCharacter.Id);
        Assert.Equal("Test", returnedCharacter.Name);
        Assert.Equal("Fighter", returnedCharacter.Class);
        Assert.Equal("Human", returnedCharacter.Species);
    }

    [Fact]
    public async Task GetCharacterById_ShouldReturnNotFound_WhenCharacterDoesNotExist()
    {
        ClearDatabase();
        var response = await _client.GetAsync("/api/characters/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCharacter_ShouldReturnCreatedCharacter_WhenValidCharacterProvided()
    {
        ClearDatabase();
        var character = new Character 
        { 
            Id = 1, 
            Name = "New Character", 
            Class = "Rogue", 
            Species = "Halfling",
            Sheet = new CharacterSheet()
        };
        var json = JsonSerializer.Serialize(character);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/characters", content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdCharacter = JsonSerializer.Deserialize<Character>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(createdCharacter);
        Assert.Equal(1, createdCharacter.Id);
        Assert.Equal("New Character", createdCharacter.Name);
        Assert.Equal("Rogue", createdCharacter.Class);
        Assert.Equal("Halfling", createdCharacter.Species);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDb>();
            var savedCharacter = db.Characters.Find(1);
            Assert.NotNull(savedCharacter);
            Assert.Equal("New Character", savedCharacter.Name);
        }
    }

    [Fact]
    public async Task UpdateCharacter_ShouldReturnUpdatedCharacter_WhenCharacterExists()
    {
        ClearDatabase();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDb>();
            db.Characters.Add(new Character { Id = 1, Name = "Original", Class = "Fighter", Species = "Human" });
            db.SaveChanges();
        }
        var updatedCharacter = new Character 
        { 
            Id = 1, 
            Name = "Updated", 
            Class = "Wizard", 
            Species = "Elf",
            Sheet = new CharacterSheet()
        };
        var json = JsonSerializer.Serialize(updatedCharacter);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/characters/1", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var returnedCharacter = JsonSerializer.Deserialize<Character>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(returnedCharacter);
        Assert.Equal(1, returnedCharacter.Id);
        Assert.Equal("Updated", returnedCharacter.Name);
        Assert.Equal("Wizard", returnedCharacter.Class);
        Assert.Equal("Elf", returnedCharacter.Species);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDb>();
            var savedCharacter = db.Characters.Find(1);
            Assert.NotNull(savedCharacter);
            Assert.Equal("Updated", savedCharacter.Name);
            Assert.Equal("Wizard", savedCharacter.Class);
            Assert.Equal("Elf", savedCharacter.Species);
        }
    }

    [Fact]
    public async Task UpdateCharacter_ShouldReturnNotFound_WhenCharacterDoesNotExist()
    {
        ClearDatabase();
        var character = new Character { Id = 999, Name = "Test", Class = "Fighter", Species = "Human" };
        var json = JsonSerializer.Serialize(character);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync("/api/characters/999", content);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCharacter_ShouldReturnDeletedCharacter_WhenCharacterExists()
    {
        ClearDatabase();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDb>();
            db.Characters.Add(new Character { Id = 1, Name = "To Delete", Class = "Fighter", Species = "Human" });
            db.SaveChanges();
        }
        var response = await _client.DeleteAsync("/api/characters/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var deletedCharacter = JsonSerializer.Deserialize<Character>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(deletedCharacter);
        Assert.Equal(1, deletedCharacter.Id);
        Assert.Equal("To Delete", deletedCharacter.Name);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDb>();
            var savedCharacter = db.Characters.Find(1);
            Assert.Null(savedCharacter);
        }
    }

    [Fact]
    public async Task DeleteCharacter_ShouldReturnNotFound_WhenCharacterDoesNotExist()
    {
        ClearDatabase();
        var response = await _client.DeleteAsync("/api/characters/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCharacter_ShouldHandleCharacterSheet_WhenSheetIsProvided()
    {
        ClearDatabase();
        var character = new Character 
        { 
            Id = 1, 
            Name = "Character with Sheet", 
            Class = "Paladin", 
            Species = "Dwarf",
            Sheet = new CharacterSheet
            {
                CharacterInfo = new CharacterInfo { CharacterName = "Character with Sheet" },
                AbilityScores = new AbilityScores 
                { 
                    Strength = new AbilityScore { Score = 16, Modifier = 3 },
                    Dexterity = new AbilityScore { Score = 14, Modifier = 2 },
                    Constitution = new AbilityScore { Score = 15, Modifier = 2 }
                },
                Combat = new Combat { ArmorClass = 18, Initiative = 2 }
            }
        };
        var json = JsonSerializer.Serialize(character);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/characters", content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdCharacter = JsonSerializer.Deserialize<Character>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(createdCharacter);
        Assert.NotNull(createdCharacter.Sheet);
        Assert.Equal("Character with Sheet", createdCharacter.Sheet.CharacterInfo.CharacterName);
        Assert.Equal(16, createdCharacter.Sheet.AbilityScores.Strength.Score);
        Assert.Equal(18, createdCharacter.Sheet.Combat.ArmorClass);
    }
} 