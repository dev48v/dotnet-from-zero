// ================================================================
// STEP 5 — EF Core entity stored in SQLite.
// One row per APOD the user has bookmarked. We persist enough of
// the original picture (title, url, mediaType) so the favourites
// page can render without going back to NASA every time.
// ================================================================
using System.ComponentModel.DataAnnotations;

namespace SpaceExplorer.Models;

public class Favourite
{
    // Auto-incremented primary key. EF Core convention picks up
    // properties named `Id` and configures them as the PK + IDENTITY.
    public int Id { get; set; }

    // The APOD calendar date — used as the natural unique key,
    // enforced by an index in AppDbContext. NASA serves at most
    // one picture per date, so this is the right uniqueness rule.
    [Required, StringLength(10)]
    public string Date { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Url { get; set; } = string.Empty;

    [StringLength(20)]
    public string MediaType { get; set; } = "image";

    // When the user clicked "Save". UTC so the value is stable
    // across machines and time zones.
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
