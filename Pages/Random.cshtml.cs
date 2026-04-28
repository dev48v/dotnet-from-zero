// ================================================================
// STEP 7 — Random APOD gallery.
// Asks the service for N random pictures, default 6.
// ================================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpaceExplorer.Models;
using SpaceExplorer.Services;

namespace SpaceExplorer.Pages;

public class RandomModel : PageModel
{
    private readonly INasaApodService _nasa;

    public RandomModel(INasaApodService nasa) => _nasa = nasa;

    [BindProperty(SupportsGet = true)]
    public int Count { get; set; } = 6;

    public IReadOnlyList<ApodPicture> Pictures { get; private set; } = Array.Empty<ApodPicture>();
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        try
        {
            Pictures = await _nasa.GetRandomAsync(Count, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = $"Could not reach NASA: {ex.Message}";
        }
    }
}
