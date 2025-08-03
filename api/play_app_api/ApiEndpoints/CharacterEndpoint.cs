using Microsoft.EntityFrameworkCore;
using play_app_api.Data;

namespace play_app_api.ApiEndpoints;

public static class CharacterEndpoint
{
    public static void MapCharacterEndpoints(this IEndpointRouteBuilder app)
    {
        var chars = app.MapGroup("api/characters");
        
        //List 
        chars.MapGet("/", async (AppDb db) => await db.Characters.ToListAsync());
        //Get by id
        chars.MapGet("/{id;int}", async (AppDb db, string id) => await db.Characters.FindAsync(id) is {} c ? Results.Ok(c) : Results.NotFound());
        //Create
        chars.MapPost("/", async (AppDb db, Character character) => {
            await db.Characters.AddAsync(character);
            await db.SaveChangesAsync();
            return Results.Created($"/api/characters/{character.Id}", character);
        });
        //Update
        chars.MapPut("/{id;int}", async (AppDb db, string id, Character character) => {
            var c = await db.Characters.FindAsync(id);
            if (c is null) return Results.NotFound();
            c.Name = character.Name;
            c.Class = character.Class;
            c.Species = character.Species;
            c.Sheet = character.Sheet;
            await db.SaveChangesAsync();
            return Results.Ok(c);
        });
        //Delete
        chars.MapDelete("/{id;int}", async (AppDb db, string id) => {
            var c = await db.Characters.FindAsync(id);
            if (c is null) return Results.NotFound();
            db.Characters.Remove(c);
            await db.SaveChangesAsync();
            return Results.Ok(c);
        });
        

        
    }
}