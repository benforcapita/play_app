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
    public DbSet<CharacterSheet> CharacterSheets => Set<CharacterSheet>();
    public DbSet<CharacterInfo> CharacterInfos => Set<CharacterInfo>();
    public DbSet<Appearance> Appearances => Set<Appearance>();
    public DbSet<AbilityScores> AbilityScoresSet => Set<AbilityScores>();
    public DbSet<AbilityScore> AbilityScoreValues => Set<AbilityScore>();
    public DbSet<SavingThrows> SavingThrowsSet => Set<SavingThrows>();
    public DbSet<SavingThrow> SavingThrowValues => Set<SavingThrow>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Combat> Combats => Set<Combat>();
    public DbSet<HitPoints> HitPoints => Set<HitPoints>();
    public DbSet<HitDice> HitDices => Set<HitDice>();
    public DbSet<DeathSaves> DeathSaves => Set<DeathSaves>();
    public DbSet<PassiveScores> PassiveScores => Set<PassiveScores>();
    public DbSet<Proficiencies> Proficiencies => Set<Proficiencies>();
    public DbSet<FeatureTrait> FeaturesAndTraits => Set<FeatureTrait>();
    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<CarryingCapacity> CarryingCapacities => Set<CarryingCapacity>();
    public DbSet<Spellcasting> Spellcastings => Set<Spellcasting>();
    public DbSet<SpellSlots> SpellSlots => Set<SpellSlots>();
    public DbSet<Slot> Slots => Set<Slot>();
    public DbSet<Spell> Spells => Set<Spell>();
    public DbSet<SpellAttack> SpellAttacks => Set<SpellAttack>();
    public DbSet<SpellSave> SpellSaves => Set<SpellSave>();
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Backstory> Backstories => Set<Backstory>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var character = modelBuilder.Entity<Character>();
        character.HasOne(c => c.Sheet)
                 .WithOne(cs => cs.Character)
                 .HasForeignKey<CharacterSheet>(cs => cs.CharacterId)
                 .OnDelete(DeleteBehavior.Cascade);

        var characterSheet = modelBuilder.Entity<CharacterSheet>();
        characterSheet.HasOne(cs => cs.CharacterInfo)
                      .WithOne(ci => ci.CharacterSheet)
                      .HasForeignKey<CharacterInfo>(ci => ci.CharacterSheetId)
                      .OnDelete(DeleteBehavior.Cascade);
        characterSheet.HasOne(cs => cs.Appearance)
                      .WithOne(a => a.CharacterSheet)
                      .HasForeignKey<Appearance>(a => a.CharacterSheetId)
                      .OnDelete(DeleteBehavior.Cascade);
        characterSheet.HasOne(cs => cs.AbilityScores)
                      .WithOne(abs => abs.CharacterSheet)
                      .HasForeignKey<AbilityScores>(abs => abs.CharacterSheetId)
                      .OnDelete(DeleteBehavior.Cascade);
        characterSheet.HasOne(cs => cs.SavingThrows)
                      .WithOne(st => st.CharacterSheet)
                      .HasForeignKey<SavingThrows>(st => st.CharacterSheetId)
                      .OnDelete(DeleteBehavior.Cascade);
        characterSheet.HasMany(cs => cs.Skills)
                      .WithOne(s => s.CharacterSheet)
                      .HasForeignKey(s => s.CharacterSheetId)
                      .OnDelete(DeleteBehavior.Cascade);
        characterSheet.HasOne(cs => cs.Combat)
                      .WithOne(c => c.CharacterSheet)
                      .HasForeignKey<Combat>(c => c.CharacterSheetId)
                      .OnDelete(DeleteBehavior.Cascade);
        characterSheet.HasOne(cs => cs.Proficiencies)
                      .WithOne(p => p.CharacterSheet)
                      .HasForeignKey<Proficiencies>(p => p.CharacterSheetId)
                      .OnDelete(DeleteBehavior.Cascade);
        characterSheet.HasMany(cs => cs.FeaturesAndTraits)
                      .WithOne(ft => ft.CharacterSheet)
                      .HasForeignKey(ft => ft.CharacterSheetId)
                      .OnDelete(DeleteBehavior.Cascade);
        characterSheet.HasOne(cs => cs.Equipment)
                      .WithOne(e => e.CharacterSheet)
                      .HasForeignKey<Equipment>(e => e.CharacterSheetId)
                      .OnDelete(DeleteBehavior.Cascade);
        characterSheet.HasOne(cs => cs.Spellcasting)
                      .WithOne(s => s.CharacterSheet)
                      .HasForeignKey<Spellcasting>(s => s.CharacterSheetId)
                      .OnDelete(DeleteBehavior.Cascade);
        characterSheet.HasOne(cs => cs.Persona)
                      .WithOne(p => p.CharacterSheet)
                      .HasForeignKey<Persona>(p => p.CharacterSheetId)
                      .OnDelete(DeleteBehavior.Cascade);
        characterSheet.HasOne(cs => cs.Backstory)
                      .WithOne(b => b.CharacterSheet)
                      .HasForeignKey<Backstory>(b => b.CharacterSheetId)
                      .OnDelete(DeleteBehavior.Cascade);

        // Configure AbilityScores relationships with distinct foreign keys
        var abilityScores = modelBuilder.Entity<AbilityScores>();
        abilityScores.HasOne(abs => abs.Strength)
                     .WithMany()
                     .HasForeignKey(abs => abs.StrengthId)
                     .OnDelete(DeleteBehavior.Cascade);
        abilityScores.HasOne(abs => abs.Dexterity)
                     .WithMany()
                     .HasForeignKey(abs => abs.DexterityId)
                     .OnDelete(DeleteBehavior.Cascade);
        abilityScores.HasOne(abs => abs.Constitution)
                     .WithMany()
                     .HasForeignKey(abs => abs.ConstitutionId)
                     .OnDelete(DeleteBehavior.Cascade);
        abilityScores.HasOne(abs => abs.Intelligence)
                     .WithMany()
                     .HasForeignKey(abs => abs.IntelligenceId)
                     .OnDelete(DeleteBehavior.Cascade);
        abilityScores.HasOne(abs => abs.Wisdom)
                     .WithMany()
                     .HasForeignKey(abs => abs.WisdomId)
                     .OnDelete(DeleteBehavior.Cascade);
        abilityScores.HasOne(abs => abs.Charisma)
                     .WithMany()
                     .HasForeignKey(abs => abs.CharismaId)
                     .OnDelete(DeleteBehavior.Cascade);

        // Configure SavingThrows relationships with distinct foreign keys
        var savingThrows = modelBuilder.Entity<SavingThrows>();
        savingThrows.HasOne(st => st.Strength)
                    .WithMany()
                    .HasForeignKey(st => st.StrengthSaveId)
                    .OnDelete(DeleteBehavior.Cascade);
        savingThrows.HasOne(st => st.Dexterity)
                    .WithMany()
                    .HasForeignKey(st => st.DexteritySaveId)
                    .OnDelete(DeleteBehavior.Cascade);
        savingThrows.HasOne(st => st.Constitution)
                    .WithMany()
                    .HasForeignKey(st => st.ConstitutionSaveId)
                    .OnDelete(DeleteBehavior.Cascade);
        savingThrows.HasOne(st => st.Intelligence)
                    .WithMany()
                    .HasForeignKey(st => st.IntelligenceSaveId)
                    .OnDelete(DeleteBehavior.Cascade);
        savingThrows.HasOne(st => st.Wisdom)
                    .WithMany()
                    .HasForeignKey(st => st.WisdomSaveId)
                    .OnDelete(DeleteBehavior.Cascade);
        savingThrows.HasOne(st => st.Charisma)
                    .WithMany()
                    .HasForeignKey(st => st.CharismaSaveId)
                    .OnDelete(DeleteBehavior.Cascade);

        var combat = modelBuilder.Entity<Combat>();
        combat.HasOne(c => c.HitPoints)
              .WithOne(hp => hp.Combat)
              .HasForeignKey<HitPoints>(hp => hp.CombatId)
              .OnDelete(DeleteBehavior.Cascade);
        combat.HasOne(c => c.HitDice)
              .WithOne(hd => hd.Combat)
              .HasForeignKey<HitDice>(hd => hd.CombatId)
              .OnDelete(DeleteBehavior.Cascade);
        combat.HasOne(c => c.DeathSaves)
              .WithOne(ds => ds.Combat)
              .HasForeignKey<DeathSaves>(ds => ds.CombatId)
              .OnDelete(DeleteBehavior.Cascade);
        combat.HasOne(c => c.PassiveScores)
              .WithOne(ps => ps.Combat)
              .HasForeignKey<PassiveScores>(ps => ps.CombatId)
              .OnDelete(DeleteBehavior.Cascade);

        var equipment = modelBuilder.Entity<Equipment>();
        equipment.HasMany(e => e.Items)
                 .WithOne(i => i.Equipment)
                 .HasForeignKey(i => i.EquipmentId)
                 .OnDelete(DeleteBehavior.Cascade);
        equipment.HasOne(e => e.Currency)
                 .WithOne(c => c.Equipment)
                 .HasForeignKey<Currency>(c => c.EquipmentId)
                 .OnDelete(DeleteBehavior.Cascade);
        equipment.HasOne(e => e.CarryingCapacity)
                 .WithOne(cc => cc.Equipment)
                 .HasForeignKey<CarryingCapacity>(cc => cc.EquipmentId)
                 .OnDelete(DeleteBehavior.Cascade);

        var spellcasting = modelBuilder.Entity<Spellcasting>();
        spellcasting.HasOne(s => s.SpellSlots)
            .WithMany()
            .HasForeignKey(s => s.SpellSlotsId)
            .OnDelete(DeleteBehavior.Cascade);
        spellcasting.HasMany(s => s.Cantrips)
            .WithOne(sp => sp.CantripSpellcasting)
            .HasForeignKey(sp => sp.CantripSpellcastingId)
            .OnDelete(DeleteBehavior.Cascade);
           spellcasting.HasMany(s => s.SpellsKnown)
            .WithOne(sp => sp.SpellsKnownSpellcasting)
            .HasForeignKey(sp => sp.SpellsKnownSpellcastingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure SpellSlots relationships with distinct foreign keys
        var spellSlots = modelBuilder.Entity<SpellSlots>();
        spellSlots.HasOne(ss => ss.Level1)
                  .WithMany()
                  .HasForeignKey(ss => ss.Level1Id)
                  .OnDelete(DeleteBehavior.Cascade);
        spellSlots.HasOne(ss => ss.Level2)
                  .WithMany()
                  .HasForeignKey(ss => ss.Level2Id)
                  .OnDelete(DeleteBehavior.Cascade);
        spellSlots.HasOne(ss => ss.Level3)
                  .WithMany()
                  .HasForeignKey(ss => ss.Level3Id)
                  .OnDelete(DeleteBehavior.Cascade);
        spellSlots.HasOne(ss => ss.Level4)
                  .WithMany()
                  .HasForeignKey(ss => ss.Level4Id)
                  .OnDelete(DeleteBehavior.Cascade);
        spellSlots.HasOne(ss => ss.Level5)
                  .WithMany()
                  .HasForeignKey(ss => ss.Level5Id)
                  .OnDelete(DeleteBehavior.Cascade);
        spellSlots.HasOne(ss => ss.Level6)
                  .WithMany()
                  .HasForeignKey(ss => ss.Level6Id)
                  .OnDelete(DeleteBehavior.Cascade);
        spellSlots.HasOne(ss => ss.Level7)
                  .WithMany()
                  .HasForeignKey(ss => ss.Level7Id)
                  .OnDelete(DeleteBehavior.Cascade);
        spellSlots.HasOne(ss => ss.Level8)
                  .WithMany()
                  .HasForeignKey(ss => ss.Level8Id)
                  .OnDelete(DeleteBehavior.Cascade);
        spellSlots.HasOne(ss => ss.Level9)
                  .WithMany()
                  .HasForeignKey(ss => ss.Level9Id)
                  .OnDelete(DeleteBehavior.Cascade);

        var spell = modelBuilder.Entity<Spell>();
        spell.HasOne(s => s.Attack)
             .WithMany()
             .HasForeignKey(s => s.AttackId)
             .OnDelete(DeleteBehavior.Cascade);
        spell.HasOne(s => s.Save)
             .WithMany()
             .HasForeignKey(s => s.SaveId)
             .OnDelete(DeleteBehavior.Cascade);

        var job = modelBuilder.Entity<ExtractionJob>();
        job.HasKey(j => j.Id);
        job.HasIndex(j => j.JobToken).IsUnique();
        job.HasIndex(j => new { j.JobToken, j.OwnerId }).IsUnique();
        job.HasIndex(j => new { j.OwnerId, j.Status, j.CreatedAt });
        job.HasMany(j => j.SectionResults)
           .WithOne(s => s.ExtractionJob)
           .HasForeignKey(s => s.ExtractionJobId)
           .OnDelete(DeleteBehavior.Cascade);
        job.HasOne(j => j.ResultCharacter)
           .WithMany()
           .HasForeignKey(j => j.ResultCharacterId)
           .OnDelete(DeleteBehavior.SetNull);

        var section = modelBuilder.Entity<SectionResult>();
        section.HasKey(s => s.Id);
    }
}