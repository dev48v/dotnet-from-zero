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

internal static class ApodErrorMapping
{
    // NASA's DEMO_KEY rate-limits at 30/hour per IP. On shared
    // platform IPs (Render free tier, etc.) we burn through it
    // fast. Map upstream failures to a 503 with a clear message
    // instead of letting the exception bubble to a 500 page.
    public static IResult ToProblem(Exception ex) =>
        Results.Problem(
            title: "NASA APOD upstream error",
            detail: ex.Message,
            statusCode: StatusCodes.Status503ServiceUnavailable);
}

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

            try
            {
                var picture = await nasa.GetByDateAsync(parsed, ct);
                return Results.Ok(picture);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return ApodErrorMapping.ToProblem(ex);
            }
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
            try
            {
                var pictures = await nasa.GetRandomAsync(count ?? 5, ct);
                return Results.Ok(pictures);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return ApodErrorMapping.ToProblem(ex);
            }
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

            try
            {
                var pictures = await nasa.GetRangeAsync(startDate, endDate, ct);
                return Results.Ok(pictures);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return ApodErrorMapping.ToProblem(ex);
            }
        })
        .WithName("GetApodRange")
        .Produces<IReadOnlyList<ApodPicture>>();

        return routes;
    }
}
