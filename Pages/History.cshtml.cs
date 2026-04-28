// ================================================================
// STEP 7 — Date-range browser.
// Defaults to "last 7 days ending yesterday" — APOD is published
// in US/Eastern, so today's image isn't always live yet at 04:00
// UTC. Yesterday-as-end avoids a confusing 404 first thing in the
// morning.
// ================================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpaceExplorer.Models;
using SpaceExplorer.Services;

namespace SpaceExplorer.Pages;

public class HistoryModel : PageModel
{
    private readonly INasaApodService _nasa;

    public HistoryModel(INasaApodService nasa) => _nasa = nasa;

    [BindProperty(SupportsGet = true)]
    public string? Start { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? End { get; set; }

    public IReadOnlyList<ApodPicture> Pictures { get; private set; } = Array.Empty<ApodPicture>();
    public string? ErrorMessage { get; private set; }
    public DateOnly EffectiveStart { get; private set; }
    public DateOnly EffectiveEnd { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        EffectiveEnd   = DateOnly.TryParse(End,   out var e) ? e : today.AddDays(-1);
        EffectiveStart = DateOnly.TryParse(Start, out var s) ? s : EffectiveEnd.AddDays(-6);

        // Same 30-day cap as the API, applied here so the form
        // can't accidentally make a huge request.
        if ((EffectiveEnd.DayNumber - EffectiveStart.DayNumber) > 30)
            EffectiveStart = EffectiveEnd.AddDays(-30);

        try
        {
            Pictures = (await _nasa.GetRangeAsync(EffectiveStart, EffectiveEnd, ct))
                .OrderByDescending(p => p.Date)
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Could not reach NASA: {ex.Message}";
        }
    }
}
