using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using play_app_api;

namespace play_app_api.Data;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }

    public DbSet<Character> Characters => Set<Character>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var c = modelBuilder.Entity<Character>();
        // Treat Character.Sheet as an owned object graph
        var opts = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        c.Property(x => x.Sheet)
            .HasConversion(
                v => JsonSerializer.Serialize(v, opts),          // to store
                v => JsonSerializer.Deserialize<CharacterSheet>(v, opts) ?? new CharacterSheet())
            .HasColumnType("json");
    }
}