using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using play_app_api;

namespace play_app_api.Data;

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }

    public DbSet<Character> Characters => Set<Character>();
    public DbSet<ExtractionJob> ExtractionJobs => Set<ExtractionJob>();
    public DbSet<SectionResult> SectionResults => Set<SectionResult>();
    
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

        // Configure ExtractionJob entity
        var job = modelBuilder.Entity<ExtractionJob>();
        job.HasKey(j => j.Id);
        job.HasIndex(j => j.JobToken).IsUnique();
        job.HasMany(j => j.SectionResults)
           .WithOne(s => s.ExtractionJob)
           .HasForeignKey(s => s.ExtractionJobId)
           .OnDelete(DeleteBehavior.Cascade);
        job.HasOne(j => j.ResultCharacter)
           .WithMany()
           .HasForeignKey(j => j.ResultCharacterId)
           .OnDelete(DeleteBehavior.SetNull);

        // Configure SectionResult entity  
        var section = modelBuilder.Entity<SectionResult>();
        section.HasKey(s => s.Id);
    }
}