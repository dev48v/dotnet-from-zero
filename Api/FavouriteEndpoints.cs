// ================================================================
// STEP 6 — Favourites CRUD endpoints under /api/favourites/*.
// Same minimal-API pattern as Step 4. EF Core's DbContext is
// injected per request; no manual SQL anywhere.
// ================================================================
using Microsoft.EntityFrameworkCore;
using SpaceExplorer.Data;
using SpaceExplorer.Models;

namespace SpaceExplorer.Api;

// Body shape for POST. Lighter than the full Favourite entity so
// the client doesn't need to know about Id / SavedAt.
public record SaveFavouriteRequest(string Date, string Title, string Url, string MediaType);

public static class FavouriteEndpoints
{
    public static IEndpointRouteBuilder MapFavouriteEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/favourites").WithTags("Favourites");

        // ---- GET /api/favourites ----
        // Newest first so the favourites page shows the latest
        // bookmark at the top without any client-side sorting.
        group.MapGet("/", async (AppDbContext db, CancellationToken ct) =>
        {
            var rows = await db.Favourites
                .OrderByDescending(f => f.SavedAt)
                .ToListAsync(ct);
            return Results.Ok(rows);
        })
        .WithName("ListFavourites")
        .Produces<List<Favourite>>();

        // ---- POST /api/favourites ----
        // Idempotent on the (unique) Date column: if the user
        // tries to bookmark the same APOD twice, return the
        // existing row with 200 OK instead of failing on the
        // unique-index constraint.
        group.MapPost("/", async (
            SaveFavouriteRequest body,
            AppDbContext db,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(body.Date) ||
                string.IsNullOrWhiteSpace(body.Title) ||
                string.IsNullOrWhiteSpace(body.Url))
            {
                return Results.BadRequest(new { error = "date, title and url are required" });
            }

            var existing = await db.Favourites
                .FirstOrDefaultAsync(f => f.Date == body.Date, ct);
            if (existing is not null) return Results.Ok(existing);

            var entity = new Favourite
            {
                Date      = body.Date,
                Title     = body.Title,
                Url       = body.Url,
                MediaType = string.IsNullOrWhiteSpace(body.MediaType) ? "image" : body.MediaType,
                SavedAt   = DateTime.UtcNow,
            };

            db.Favourites.Add(entity);
            await db.SaveChangesAsync(ct);

            // 201 Created with a Location header pointing at the
            // resource — what every REST style guide asks for.
            return Results.Created($"/api/favourites/{entity.Id}", entity);
        })
        .WithName("SaveFavourite")
        .Produces<Favourite>(StatusCodes.Status201Created)
        .Produces<Favourite>(StatusCodes.Status200OK);

        // ---- DELETE /api/favourites/{id} ----
        group.MapDelete("/{id:int}", async (
            int id,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var row = await db.Favourites.FindAsync(new object?[] { id }, ct);
            if (row is null) return Results.NotFound();

            db.Favourites.Remove(row);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        })
        .WithName("DeleteFavourite");

        return routes;
    }
}
