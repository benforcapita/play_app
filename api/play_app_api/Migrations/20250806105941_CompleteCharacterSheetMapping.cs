using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace play_app_api.Migrations
{
    /// <inheritdoc />
    public partial class CompleteCharacterSheetMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AbilityScoreValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Modifier = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbilityScoreValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<string>(type: "text", nullable: false),
                    Species = table.Column<string>(type: "text", nullable: false),
                    CharacterSheetId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SavingThrowValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Proficient = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingThrowValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Slots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Total = table.Column<int>(type: "integer", nullable: false),
                    Used = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpellAttacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Bonus = table.Column<int>(type: "integer", nullable: false),
                    Damage = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpellAttacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpellSaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ability = table.Column<string>(type: "text", nullable: false),
                    Dc = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpellSaves", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CharacterSheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    CharacterId1 = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterSheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterSheets_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterSheets_Characters_CharacterId1",
                        column: x => x.CharacterId1,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExtractionJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobToken = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    FileDataUrl = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ResultCharacterId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractionJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtractionJobs_Characters_ResultCharacterId",
                        column: x => x.ResultCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SpellSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Level1Id = table.Column<int>(type: "integer", nullable: false),
                    Level2Id = table.Column<int>(type: "integer", nullable: false),
                    Level3Id = table.Column<int>(type: "integer", nullable: false),
                    Level4Id = table.Column<int>(type: "integer", nullable: false),
                    Level5Id = table.Column<int>(type: "integer", nullable: false),
                    Level6Id = table.Column<int>(type: "integer", nullable: false),
                    Level7Id = table.Column<int>(type: "integer", nullable: false),
                    Level8Id = table.Column<int>(type: "integer", nullable: false),
                    Level9Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpellSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpellSlots_Slots_Level1Id",
                        column: x => x.Level1Id,
                        principalTable: "Slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpellSlots_Slots_Level2Id",
                        column: x => x.Level2Id,
                        principalTable: "Slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpellSlots_Slots_Level3Id",
                        column: x => x.Level3Id,
                        principalTable: "Slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpellSlots_Slots_Level4Id",
                        column: x => x.Level4Id,
                        principalTable: "Slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpellSlots_Slots_Level5Id",
                        column: x => x.Level5Id,
                        principalTable: "Slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpellSlots_Slots_Level6Id",
                        column: x => x.Level6Id,
                        principalTable: "Slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpellSlots_Slots_Level7Id",
                        column: x => x.Level7Id,
                        principalTable: "Slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpellSlots_Slots_Level8Id",
                        column: x => x.Level8Id,
                        principalTable: "Slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpellSlots_Slots_Level9Id",
                        column: x => x.Level9Id,
                        principalTable: "Slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbilityScoresSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterSheetId = table.Column<int>(type: "integer", nullable: false),
                    CharacterSheetId1 = table.Column<int>(type: "integer", nullable: false),
                    StrengthId = table.Column<int>(type: "integer", nullable: false),
                    DexterityId = table.Column<int>(type: "integer", nullable: false),
                    ConstitutionId = table.Column<int>(type: "integer", nullable: false),
                    IntelligenceId = table.Column<int>(type: "integer", nullable: false),
                    WisdomId = table.Column<int>(type: "integer", nullable: false),
                    CharismaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbilityScoresSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbilityScoresSet_AbilityScoreValues_CharismaId",
                        column: x => x.CharismaId,
                        principalTable: "AbilityScoreValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbilityScoresSet_AbilityScoreValues_ConstitutionId",
                        column: x => x.ConstitutionId,
                        principalTable: "AbilityScoreValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbilityScoresSet_AbilityScoreValues_DexterityId",
                        column: x => x.DexterityId,
                        principalTable: "AbilityScoreValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbilityScoresSet_AbilityScoreValues_IntelligenceId",
                        column: x => x.IntelligenceId,
                        principalTable: "AbilityScoreValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbilityScoresSet_AbilityScoreValues_StrengthId",
                        column: x => x.StrengthId,
                        principalTable: "AbilityScoreValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbilityScoresSet_AbilityScoreValues_WisdomId",
                        column: x => x.WisdomId,
                        principalTable: "AbilityScoreValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbilityScoresSet_CharacterSheets_CharacterSheetId",
                        column: x => x.CharacterSheetId,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbilityScoresSet_CharacterSheets_CharacterSheetId1",
                        column: x => x.CharacterSheetId1,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appearances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterSheetId = table.Column<int>(type: "integer", nullable: false),
                    CharacterSheetId1 = table.Column<int>(type: "integer", nullable: false),
                    Size = table.Column<string>(type: "text", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    Age = table.Column<string>(type: "text", nullable: false),
                    Height = table.Column<string>(type: "text", nullable: false),
                    Weight = table.Column<string>(type: "text", nullable: false),
                    Skin = table.Column<string>(type: "text", nullable: false),
                    Eyes = table.Column<string>(type: "text", nullable: false),
                    Hair = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appearances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appearances_CharacterSheets_CharacterSheetId",
                        column: x => x.CharacterSheetId,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appearances_CharacterSheets_CharacterSheetId1",
                        column: x => x.CharacterSheetId1,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Backstories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterSheetId = table.Column<int>(type: "integer", nullable: false),
                    CharacterSheetId1 = table.Column<int>(type: "integer", nullable: false),
                    Background = table.Column<string>(type: "text", nullable: false),
                    Faction = table.Column<string>(type: "text", nullable: false),
                    Origin = table.Column<string>(type: "text", nullable: false),
                    SignificantOthers = table.Column<string>(type: "text", nullable: false),
                    ImportantEvents = table.Column<string>(type: "text", nullable: false),
                    AlliesAndEnemies = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backstories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Backstories_CharacterSheets_CharacterSheetId",
                        column: x => x.CharacterSheetId,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Backstories_CharacterSheets_CharacterSheetId1",
                        column: x => x.CharacterSheetId1,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterSheetId = table.Column<int>(type: "integer", nullable: false),
                    CharacterSheetId1 = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Class = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Species = table.Column<string>(type: "text", nullable: false),
                    Subclass = table.Column<string>(type: "text", nullable: false),
                    Alignment = table.Column<string>(type: "text", nullable: false),
                    Experience = table.Column<int>(type: "integer", nullable: false),
                    NextLevelXp = table.Column<int>(type: "integer", nullable: false),
                    CharacterName = table.Column<string>(type: "text", nullable: false),
                    ClassAndLevel = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterInfos_CharacterSheets_CharacterSheetId",
                        column: x => x.CharacterSheetId,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterInfos_CharacterSheets_CharacterSheetId1",
                        column: x => x.CharacterSheetId1,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Combats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterSheetId = table.Column<int>(type: "integer", nullable: false),
                    CharacterSheetId1 = table.Column<int>(type: "integer", nullable: false),
                    ArmorClass = table.Column<int>(type: "integer", nullable: false),
                    Initiative = table.Column<int>(type: "integer", nullable: false),
                    Speed = table.Column<string>(type: "text", nullable: false),
                    ProficiencyBonus = table.Column<int>(type: "integer", nullable: false),
                    Inspiration = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Combats_CharacterSheets_CharacterSheetId",
                        column: x => x.CharacterSheetId,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Combats_CharacterSheets_CharacterSheetId1",
                        column: x => x.CharacterSheetId1,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Equipments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterSheetId = table.Column<int>(type: "integer", nullable: false),
                    CharacterSheetId1 = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipments_CharacterSheets_CharacterSheetId",
                        column: x => x.CharacterSheetId,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Equipments_CharacterSheets_CharacterSheetId1",
                        column: x => x.CharacterSheetId1,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeaturesAndTraits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterSheetId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Uses = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturesAndTraits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeaturesAndTraits_CharacterSheets_CharacterSheetId",
                        column: x => x.CharacterSheetId,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterSheetId = table.Column<int>(type: "integer", nullable: false),
                    CharacterSheetId1 = table.Column<int>(type: "integer", nullable: false),
                    PersonalityTraits = table.Column<string>(type: "text", nullable: false),
                    Ideals = table.Column<string>(type: "text", nullable: false),
                    Bonds = table.Column<string>(type: "text", nullable: false),
                    Flaws = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Personas_CharacterSheets_CharacterSheetId",
                        column: x => x.CharacterSheetId,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Personas_CharacterSheets_CharacterSheetId1",
                        column: x => x.CharacterSheetId1,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Proficiencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterSheetId = table.Column<int>(type: "integer", nullable: false),
                    CharacterSheetId1 = table.Column<int>(type: "integer", nullable: false),
                    Armor = table.Column<List<string>>(type: "text[]", nullable: false),
                    Weapons = table.Column<List<string>>(type: "text[]", nullable: false),
                    Tools = table.Column<List<string>>(type: "text[]", nullable: false),
                    Languages = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proficiencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Proficiencies_CharacterSheets_CharacterSheetId",
                        column: x => x.CharacterSheetId,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Proficiencies_CharacterSheets_CharacterSheetId1",
                        column: x => x.CharacterSheetId1,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavingThrowsSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterSheetId = table.Column<int>(type: "integer", nullable: false),
                    CharacterSheetId1 = table.Column<int>(type: "integer", nullable: false),
                    StrengthSaveId = table.Column<int>(type: "integer", nullable: false),
                    DexteritySaveId = table.Column<int>(type: "integer", nullable: false),
                    ConstitutionSaveId = table.Column<int>(type: "integer", nullable: false),
                    IntelligenceSaveId = table.Column<int>(type: "integer", nullable: false),
                    WisdomSaveId = table.Column<int>(type: "integer", nullable: false),
                    CharismaSaveId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavingThrowsSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavingThrowsSet_CharacterSheets_CharacterSheetId",
                        column: x => x.CharacterSheetId,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavingThrowsSet_CharacterSheets_CharacterSheetId1",
                        column: x => x.CharacterSheetId1,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavingThrowsSet_SavingThrowValues_CharismaSaveId",
                        column: x => x.CharismaSaveId,
                        principalTable: "SavingThrowValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavingThrowsSet_SavingThrowValues_ConstitutionSaveId",
                        column: x => x.ConstitutionSaveId,
                        principalTable: "SavingThrowValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavingThrowsSet_SavingThrowValues_DexteritySaveId",
                        column: x => x.DexteritySaveId,
                        principalTable: "SavingThrowValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavingThrowsSet_SavingThrowValues_IntelligenceSaveId",
                        column: x => x.IntelligenceSaveId,
                        principalTable: "SavingThrowValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavingThrowsSet_SavingThrowValues_StrengthSaveId",
                        column: x => x.StrengthSaveId,
                        principalTable: "SavingThrowValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavingThrowsSet_SavingThrowValues_WisdomSaveId",
                        column: x => x.WisdomSaveId,
                        principalTable: "SavingThrowValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterSheetId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Ability = table.Column<string>(type: "text", nullable: false),
                    Modifier = table.Column<int>(type: "integer", nullable: false),
                    Proficient = table.Column<bool>(type: "boolean", nullable: false),
                    Expertise = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Skills_CharacterSheets_CharacterSheetId",
                        column: x => x.CharacterSheetId,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SectionResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SectionName = table.Column<string>(type: "text", nullable: false),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExtractionJobId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionResults_ExtractionJobs_ExtractionJobId",
                        column: x => x.ExtractionJobId,
                        principalTable: "ExtractionJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Spellcastings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterSheetId = table.Column<int>(type: "integer", nullable: false),
                    CharacterSheetId1 = table.Column<int>(type: "integer", nullable: false),
                    Class = table.Column<string>(type: "text", nullable: false),
                    Ability = table.Column<string>(type: "text", nullable: false),
                    SaveDC = table.Column<int>(type: "integer", nullable: false),
                    AttackBonus = table.Column<int>(type: "integer", nullable: false),
                    SpellSlotsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spellcastings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spellcastings_CharacterSheets_CharacterSheetId",
                        column: x => x.CharacterSheetId,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Spellcastings_CharacterSheets_CharacterSheetId1",
                        column: x => x.CharacterSheetId1,
                        principalTable: "CharacterSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Spellcastings_SpellSlots_SpellSlotsId",
                        column: x => x.SpellSlotsId,
                        principalTable: "SpellSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeathSaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CombatId = table.Column<int>(type: "integer", nullable: false),
                    CombatId1 = table.Column<int>(type: "integer", nullable: false),
                    Successes = table.Column<int>(type: "integer", nullable: false),
                    Failures = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeathSaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeathSaves_Combats_CombatId",
                        column: x => x.CombatId,
                        principalTable: "Combats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeathSaves_Combats_CombatId1",
                        column: x => x.CombatId1,
                        principalTable: "Combats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HitDices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CombatId = table.Column<int>(type: "integer", nullable: false),
                    CombatId1 = table.Column<int>(type: "integer", nullable: false),
                    Total = table.Column<string>(type: "text", nullable: false),
                    Current = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HitDices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HitDices_Combats_CombatId",
                        column: x => x.CombatId,
                        principalTable: "Combats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HitDices_Combats_CombatId1",
                        column: x => x.CombatId1,
                        principalTable: "Combats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HitPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CombatId = table.Column<int>(type: "integer", nullable: false),
                    CombatId1 = table.Column<int>(type: "integer", nullable: false),
                    Max = table.Column<int>(type: "integer", nullable: false),
                    Current = table.Column<int>(type: "integer", nullable: false),
                    Temporary = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HitPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HitPoints_Combats_CombatId",
                        column: x => x.CombatId,
                        principalTable: "Combats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HitPoints_Combats_CombatId1",
                        column: x => x.CombatId1,
                        principalTable: "Combats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PassiveScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CombatId = table.Column<int>(type: "integer", nullable: false),
                    CombatId1 = table.Column<int>(type: "integer", nullable: false),
                    Perception = table.Column<int>(type: "integer", nullable: false),
                    Insight = table.Column<int>(type: "integer", nullable: false),
                    Investigation = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PassiveScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PassiveScores_Combats_CombatId",
                        column: x => x.CombatId,
                        principalTable: "Combats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PassiveScores_Combats_CombatId1",
                        column: x => x.CombatId1,
                        principalTable: "Combats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarryingCapacities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId1 = table.Column<int>(type: "integer", nullable: false),
                    WeightCarried = table.Column<float>(type: "real", nullable: false),
                    Encumbered = table.Column<float>(type: "real", nullable: false),
                    PushDragLift = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarryingCapacities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarryingCapacities_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarryingCapacities_Equipments_EquipmentId1",
                        column: x => x.EquipmentId1,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    EquipmentId1 = table.Column<int>(type: "integer", nullable: false),
                    Cp = table.Column<int>(type: "integer", nullable: false),
                    Sp = table.Column<int>(type: "integer", nullable: false),
                    Ep = table.Column<int>(type: "integer", nullable: false),
                    Gp = table.Column<int>(type: "integer", nullable: false),
                    Pp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Currencies_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Currencies_Equipments_EquipmentId1",
                        column: x => x.EquipmentId1,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EquipmentId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Spells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SpellcastingId = table.Column<int>(type: "integer", nullable: false),
                    SpellcastingId1 = table.Column<int>(type: "integer", nullable: false),
                    SpellType = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    CastingTime = table.Column<string>(type: "text", nullable: false),
                    Range = table.Column<string>(type: "text", nullable: false),
                    Components = table.Column<string>(type: "text", nullable: false),
                    Duration = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    AttackId = table.Column<int>(type: "integer", nullable: false),
                    SaveId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Spells_SpellAttacks_AttackId",
                        column: x => x.AttackId,
                        principalTable: "SpellAttacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Spells_SpellSaves_SaveId",
                        column: x => x.SaveId,
                        principalTable: "SpellSaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Spells_Spellcastings_SpellcastingId",
                        column: x => x.SpellcastingId,
                        principalTable: "Spellcastings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Spells_Spellcastings_SpellcastingId1",
                        column: x => x.SpellcastingId1,
                        principalTable: "Spellcastings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbilityScoresSet_CharacterSheetId",
                table: "AbilityScoresSet",
                column: "CharacterSheetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbilityScoresSet_CharacterSheetId1",
                table: "AbilityScoresSet",
                column: "CharacterSheetId1");

            migrationBuilder.CreateIndex(
                name: "IX_AbilityScoresSet_CharismaId",
                table: "AbilityScoresSet",
                column: "CharismaId");

            migrationBuilder.CreateIndex(
                name: "IX_AbilityScoresSet_ConstitutionId",
                table: "AbilityScoresSet",
                column: "ConstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_AbilityScoresSet_DexterityId",
                table: "AbilityScoresSet",
                column: "DexterityId");

            migrationBuilder.CreateIndex(
                name: "IX_AbilityScoresSet_IntelligenceId",
                table: "AbilityScoresSet",
                column: "IntelligenceId");

            migrationBuilder.CreateIndex(
                name: "IX_AbilityScoresSet_StrengthId",
                table: "AbilityScoresSet",
                column: "StrengthId");

            migrationBuilder.CreateIndex(
                name: "IX_AbilityScoresSet_WisdomId",
                table: "AbilityScoresSet",
                column: "WisdomId");

            migrationBuilder.CreateIndex(
                name: "IX_Appearances_CharacterSheetId",
                table: "Appearances",
                column: "CharacterSheetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appearances_CharacterSheetId1",
                table: "Appearances",
                column: "CharacterSheetId1");

            migrationBuilder.CreateIndex(
                name: "IX_Backstories_CharacterSheetId",
                table: "Backstories",
                column: "CharacterSheetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Backstories_CharacterSheetId1",
                table: "Backstories",
                column: "CharacterSheetId1");

            migrationBuilder.CreateIndex(
                name: "IX_CarryingCapacities_EquipmentId",
                table: "CarryingCapacities",
                column: "EquipmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarryingCapacities_EquipmentId1",
                table: "CarryingCapacities",
                column: "EquipmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterInfos_CharacterSheetId",
                table: "CharacterInfos",
                column: "CharacterSheetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterInfos_CharacterSheetId1",
                table: "CharacterInfos",
                column: "CharacterSheetId1");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheets_CharacterId",
                table: "CharacterSheets",
                column: "CharacterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSheets_CharacterId1",
                table: "CharacterSheets",
                column: "CharacterId1");

            migrationBuilder.CreateIndex(
                name: "IX_Combats_CharacterSheetId",
                table: "Combats",
                column: "CharacterSheetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Combats_CharacterSheetId1",
                table: "Combats",
                column: "CharacterSheetId1");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_EquipmentId",
                table: "Currencies",
                column: "EquipmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_EquipmentId1",
                table: "Currencies",
                column: "EquipmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_DeathSaves_CombatId",
                table: "DeathSaves",
                column: "CombatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeathSaves_CombatId1",
                table: "DeathSaves",
                column: "CombatId1");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_CharacterSheetId",
                table: "Equipments",
                column: "CharacterSheetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_CharacterSheetId1",
                table: "Equipments",
                column: "CharacterSheetId1");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractionJobs_JobToken",
                table: "ExtractionJobs",
                column: "JobToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExtractionJobs_ResultCharacterId",
                table: "ExtractionJobs",
                column: "ResultCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_FeaturesAndTraits_CharacterSheetId",
                table: "FeaturesAndTraits",
                column: "CharacterSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_HitDices_CombatId",
                table: "HitDices",
                column: "CombatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HitDices_CombatId1",
                table: "HitDices",
                column: "CombatId1");

            migrationBuilder.CreateIndex(
                name: "IX_HitPoints_CombatId",
                table: "HitPoints",
                column: "CombatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HitPoints_CombatId1",
                table: "HitPoints",
                column: "CombatId1");

            migrationBuilder.CreateIndex(
                name: "IX_Items_EquipmentId",
                table: "Items",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PassiveScores_CombatId",
                table: "PassiveScores",
                column: "CombatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PassiveScores_CombatId1",
                table: "PassiveScores",
                column: "CombatId1");

            migrationBuilder.CreateIndex(
                name: "IX_Personas_CharacterSheetId",
                table: "Personas",
                column: "CharacterSheetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personas_CharacterSheetId1",
                table: "Personas",
                column: "CharacterSheetId1");

            migrationBuilder.CreateIndex(
                name: "IX_Proficiencies_CharacterSheetId",
                table: "Proficiencies",
                column: "CharacterSheetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proficiencies_CharacterSheetId1",
                table: "Proficiencies",
                column: "CharacterSheetId1");

            migrationBuilder.CreateIndex(
                name: "IX_SavingThrowsSet_CharacterSheetId",
                table: "SavingThrowsSet",
                column: "CharacterSheetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavingThrowsSet_CharacterSheetId1",
                table: "SavingThrowsSet",
                column: "CharacterSheetId1");

            migrationBuilder.CreateIndex(
                name: "IX_SavingThrowsSet_CharismaSaveId",
                table: "SavingThrowsSet",
                column: "CharismaSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingThrowsSet_ConstitutionSaveId",
                table: "SavingThrowsSet",
                column: "ConstitutionSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingThrowsSet_DexteritySaveId",
                table: "SavingThrowsSet",
                column: "DexteritySaveId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingThrowsSet_IntelligenceSaveId",
                table: "SavingThrowsSet",
                column: "IntelligenceSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingThrowsSet_StrengthSaveId",
                table: "SavingThrowsSet",
                column: "StrengthSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_SavingThrowsSet_WisdomSaveId",
                table: "SavingThrowsSet",
                column: "WisdomSaveId");

            migrationBuilder.CreateIndex(
                name: "IX_SectionResults_ExtractionJobId",
                table: "SectionResults",
                column: "ExtractionJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_CharacterSheetId",
                table: "Skills",
                column: "CharacterSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_Spellcastings_CharacterSheetId",
                table: "Spellcastings",
                column: "CharacterSheetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spellcastings_CharacterSheetId1",
                table: "Spellcastings",
                column: "CharacterSheetId1");

            migrationBuilder.CreateIndex(
                name: "IX_Spellcastings_SpellSlotsId",
                table: "Spellcastings",
                column: "SpellSlotsId");

            migrationBuilder.CreateIndex(
                name: "IX_Spells_AttackId",
                table: "Spells",
                column: "AttackId");

            migrationBuilder.CreateIndex(
                name: "IX_Spells_SaveId",
                table: "Spells",
                column: "SaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Spells_SpellcastingId",
                table: "Spells",
                column: "SpellcastingId");

            migrationBuilder.CreateIndex(
                name: "IX_Spells_SpellcastingId1",
                table: "Spells",
                column: "SpellcastingId1");

            migrationBuilder.CreateIndex(
                name: "IX_SpellSlots_Level1Id",
                table: "SpellSlots",
                column: "Level1Id");

            migrationBuilder.CreateIndex(
                name: "IX_SpellSlots_Level2Id",
                table: "SpellSlots",
                column: "Level2Id");

            migrationBuilder.CreateIndex(
                name: "IX_SpellSlots_Level3Id",
                table: "SpellSlots",
                column: "Level3Id");

            migrationBuilder.CreateIndex(
                name: "IX_SpellSlots_Level4Id",
                table: "SpellSlots",
                column: "Level4Id");

            migrationBuilder.CreateIndex(
                name: "IX_SpellSlots_Level5Id",
                table: "SpellSlots",
                column: "Level5Id");

            migrationBuilder.CreateIndex(
                name: "IX_SpellSlots_Level6Id",
                table: "SpellSlots",
                column: "Level6Id");

            migrationBuilder.CreateIndex(
                name: "IX_SpellSlots_Level7Id",
                table: "SpellSlots",
                column: "Level7Id");

            migrationBuilder.CreateIndex(
                name: "IX_SpellSlots_Level8Id",
                table: "SpellSlots",
                column: "Level8Id");

            migrationBuilder.CreateIndex(
                name: "IX_SpellSlots_Level9Id",
                table: "SpellSlots",
                column: "Level9Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbilityScoresSet");

            migrationBuilder.DropTable(
                name: "Appearances");

            migrationBuilder.DropTable(
                name: "Backstories");

            migrationBuilder.DropTable(
                name: "CarryingCapacities");

            migrationBuilder.DropTable(
                name: "CharacterInfos");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "DeathSaves");

            migrationBuilder.DropTable(
                name: "FeaturesAndTraits");

            migrationBuilder.DropTable(
                name: "HitDices");

            migrationBuilder.DropTable(
                name: "HitPoints");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "PassiveScores");

            migrationBuilder.DropTable(
                name: "Personas");

            migrationBuilder.DropTable(
                name: "Proficiencies");

            migrationBuilder.DropTable(
                name: "SavingThrowsSet");

            migrationBuilder.DropTable(
                name: "SectionResults");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "Spells");

            migrationBuilder.DropTable(
                name: "AbilityScoreValues");

            migrationBuilder.DropTable(
                name: "Equipments");

            migrationBuilder.DropTable(
                name: "Combats");

            migrationBuilder.DropTable(
                name: "SavingThrowValues");

            migrationBuilder.DropTable(
                name: "ExtractionJobs");

            migrationBuilder.DropTable(
                name: "SpellAttacks");

            migrationBuilder.DropTable(
                name: "SpellSaves");

            migrationBuilder.DropTable(
                name: "Spellcastings");

            migrationBuilder.DropTable(
                name: "CharacterSheets");

            migrationBuilder.DropTable(
                name: "SpellSlots");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "Slots");
        }
    }
}
