// ================================================================
// STEP 7 — Favourites list page.
// Reads from the SQLite Favourites table via EF Core. POST handler
// removes a row and redirects back so the page stays a plain
// progressive-enhancement form (no JS required).
// ================================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SpaceExplorer.Data;
using SpaceExplorer.Models;

namespace SpaceExplorer.Pages;

public class FavouritesModel : PageModel
{
    private readonly AppDbContext _db;

    public FavouritesModel(AppDbContext db) => _db = db;

    public IReadOnlyList<Favourite> Items { get; private set; } = Array.Empty<Favourite>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Items = await _db.Favourites
            .AsNoTracking()
            .OrderByDescending(f => f.SavedAt)
            .ToListAsync(ct);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
    {
        var row = await _db.Favourites.FindAsync(new object?[] { id }, ct);
        if (row is not null)
        {
            _db.Favourites.Remove(row);
            await _db.SaveChangesAsync(ct);
        }
        return RedirectToPage();
    }
}
