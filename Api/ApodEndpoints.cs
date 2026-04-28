// ================================================================
// STEP 4 — Minimal API endpoints under /api/apod/*.
// Razor Pages renders HTML; this layer renders JSON. Same service,
// two surfaces. Useful for SPAs, mobile clients, or just curl.
//
// The pattern: extension method on IEndpointRouteBuilder so
// Program.cs stays a one-liner: `app.MapApodEndpoints();`.
// ================================================================
using SpaceExplorer.Models;
using SpaceExplorer.Services;

namespace SpaceExplorer.Api;

public static class ApodEndpoints
{
    public static IEndpointRouteBuilder MapApodEndpoints(this IEndpointRouteBuilder routes)
    {
        // MapGroup keeps the prefix in one place. Tagging the
        // group makes Swagger render every endpoint under "APOD".
        var group = routes.MapGroup("/api/apod").WithTags("APOD");

        // ---- /api/apod/today ----
        // Optional ?date=YYYY-MM-DD. Without it, NASA returns the
        // current US/Eastern picture of the day.
        group.MapGet("/today", async (
            string? date,
            INasaApodService nasa,
            CancellationToken ct) =>
        {
            DateOnly? parsed = null;
            if (!string.IsNullOrWhiteSpace(date))
            {
                if (!DateOnly.TryParse(date, out var d))
                    return Results.BadRequest(new { error = "date must be YYYY-MM-DD" });
                parsed = d;
            }

            var picture = await nasa.GetByDateAsync(parsed, ct);
            return Results.Ok(picture);
        })
        .WithName("GetApodByDate")
        .Produces<ApodPicture>();

        // ---- /api/apod/random?count=5 ----
        group.MapGet("/random", async (
            int? count,
            INasaApodService nasa,
            CancellationToken ct) =>
        {
            // Default 5 keeps DEMO_KEY happy while still feeling
            // useful. The service clamps to 1..20 either way.
            var pictures = await nasa.GetRandomAsync(count ?? 5, ct);
            return Results.Ok(pictures);
        })
        .WithName("GetApodRandom")
        .Produces<IReadOnlyList<ApodPicture>>();

        // ---- /api/apod/range?start=YYYY-MM-DD&end=YYYY-MM-DD ----
        group.MapGet("/range", async (
            string start,
            string end,
            INasaApodService nasa,
            CancellationToken ct) =>
        {
            if (!DateOnly.TryParse(start, out var startDate) ||
                !DateOnly.TryParse(end,   out var endDate))
            {
                return Results.BadRequest(new { error = "start and end must be YYYY-MM-DD" });
            }

            // Cap at 30 days so a typo can't drag down a month of
            // images and burn the whole rate-limit window.
            if ((endDate.DayNumber - startDate.DayNumber) > 30)
                return Results.BadRequest(new { error = "range may not exceed 30 days" });

            var pictures = await nasa.GetRangeAsync(startDate, endDate, ct);
            return Results.Ok(pictures);
        })
        .WithName("GetApodRange")
        .Produces<IReadOnlyList<ApodPicture>>();

        return routes;
    }
}
