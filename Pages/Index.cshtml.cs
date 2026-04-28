// ================================================================
// STEP 7 — PageModel for the home page ("today's APOD").
// Razor Pages binds ?date=YYYY-MM-DD via [BindProperty] and runs
// OnGetAsync() before rendering. Two handlers live here:
//   * OnGetAsync       – fetch the picture (current day if no ?date)
//   * OnPostSaveAsync  – insert a Favourite row, then redirect back.
// ================================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SpaceExplorer.Data;
using SpaceExplorer.Models;
using SpaceExplorer.Services;

namespace SpaceExplorer.Pages;

public class IndexModel : PageModel
{
    private readonly INasaApodService _nasa;
    private readonly AppDbContext _db;

    public IndexModel(INasaApodService nasa, AppDbContext db)
    {
        _nasa = nasa;
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string? Date { get; set; }

    public ApodPicture? Picture { get; private set; }
    public bool AlreadySaved { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        try
        {
            DateOnly? parsed = null;
            if (DateOnly.TryParse(Date, out var d)) parsed = d;

            Picture = await _nasa.GetByDateAsync(parsed, ct);

            // Tell the view whether the "Save" button should
            // already be in its "saved" state.
            AlreadySaved = await IsSavedAsync(Picture.Date, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // NASA rate-limits, slow networks, and DEMO_KEY caps
            // all show up as either HttpRequestException or
            // TaskCanceledException (timeout). Surface inline
            // instead of a 500.
            ErrorMessage = $"Could not reach NASA: {ex.Message}";
        }
    }

    public async Task<IActionResult> OnPostSaveAsync(
        string date, string title, string url, string mediaType,
        CancellationToken ct)
    {
        // Idempotent — see /api/favourites POST handler for the
        // same logic. We accept duplicate clicks silently.
        var exists = await IsSavedAsync(date, ct);
        if (!exists)
        {
            _db.Favourites.Add(new Favourite
            {
                Date = date,
                Title = title,
                Url = url,
                MediaType = string.IsNullOrWhiteSpace(mediaType) ? "image" : mediaType,
            });
            await _db.SaveChangesAsync(ct);
        }

        return RedirectToPage(new { date });
    }

    private async Task<bool> IsSavedAsync(string date, CancellationToken ct)
    {
        return await _db.Favourites
            .AsNoTracking()
            .AnyAsync(f => f.Date == date, ct);
    }
}
