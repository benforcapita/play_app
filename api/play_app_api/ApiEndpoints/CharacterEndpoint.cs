using Microsoft.EntityFrameworkCore;
using play_app_api.Data;
using System.Security.Claims;

namespace play_app_api.ApiEndpoints;

public static class CharacterEndpoint
{
    public static void MapCharacterEndpoints(this IEndpointRouteBuilder app)
    {
        var chars = app.MapGroup("/api/characters").RequireAuthorization("UserOnly");

        chars.MapGet("/", async (AppDb db, ClaimsPrincipal user) =>
        {
            var uid = user.FindFirstValue("uid") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var characters = await db.Characters
                .Where(c => c.OwnerId == uid)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.CharacterInfo)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
            return Results.Ok(characters);
        });

        chars.MapGet("/{id:int}", async (AppDb db, ClaimsPrincipal user, int id) =>
        {
            var uid = user.FindFirstValue("uid") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var character = await db.Characters
                .Where(c => c.OwnerId == uid && c.Id == id)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.CharacterInfo)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.Appearance)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.AbilityScores)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.SavingThrows)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.Skills)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.Combat)
                        .ThenInclude(co => co.HitPoints)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.Combat)
                        .ThenInclude(co => co.HitDice)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.Combat)
                        .ThenInclude(co => co.DeathSaves)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.Combat)
                        .ThenInclude(co => co.PassiveScores)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.Proficiencies)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.FeaturesAndTraits)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.Equipment)
                        .ThenInclude(e => e.Items)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.Equipment)
                        .ThenInclude(e => e.Currency)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.Equipment)
                        .ThenInclude(e => e.CarryingCapacity)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.Spellcasting)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.Persona)
                .Include(c => c.Sheet)
                    .ThenInclude(s => s.Backstory)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return character is not null ? Results.Ok(character) : Results.NotFound();
        });

        /*
        chars.MapPost("/", async (AppDb db, Character character) =>
        {
            await db.Characters.AddAsync(character);
            await db.SaveChangesAsync();
            return Results.Created($"/api/characters/{character.Id}", character);
        });
        */

        chars.MapPut("/{id:int}", async (AppDb db, ClaimsPrincipal user, int id, Character character) =>
        {
            var uid = user.FindFirstValue("uid") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var c = await db.Characters.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == uid);
            if (c is null) return Results.NotFound();
            c.Name = character.Name;
            c.Class = character.Class;
            c.Species = character.Species;
            c.Sheet = character.Sheet;
            await db.SaveChangesAsync();
            return Results.Ok(c);
        });

        chars.MapDelete("/{id:int}", async (AppDb db, ClaimsPrincipal user, int id) =>
        {
            var uid = user.FindFirstValue("uid") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var c = await db.Characters.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == uid);
            if (c is null) return Results.NotFound();
            db.Characters.Remove(c);
            await db.SaveChangesAsync();
            return Results.Ok(c);
        });

        chars.MapGet("/{id:int}/sheet", async (AppDb db, ClaimsPrincipal user, int id) =>
        {
            var uid = user.FindFirstValue("uid") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var character = await db.Characters
                .Where(c => c.OwnerId == uid && c.Id == id)
                .Include(c => c.Sheet) // Must include the sheet to access it
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return character?.Sheet is not null ? Results.Ok(character.Sheet) : Results.NotFound();
        });

        chars.MapGet("/{id:int}/sheet/{section}", async (AppDb db, ClaimsPrincipal user, int id, string section) =>
        {
            var uid = user.FindFirstValue("uid") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var character = await db.Characters
                .Where(c => c.OwnerId == uid && c.Id == id)
                .Include(c => c.Sheet) // We always need the sheet shell
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (character?.Sheet is null) return Results.NotFound();

            return section.ToLowerInvariant() switch
            {
                "characterinfo" => Results.Ok(character.Sheet.CharacterInfo),
                "combat" => Results.Ok(character.Sheet.Combat),
                _ => Results.BadRequest("Unknown or unloaded section")
            };
        });
    }
}