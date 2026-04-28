// ================================================================
// STEP 3 — Service contract.
// We expose only what callers need — three methods. Razor Pages,
// minimal-API endpoints, and unit tests all depend on this
// interface, never on the concrete `NasaApodService`. That swap
// is what lets us mock NASA in tests later without ever hitting
// the real network.
// ================================================================
using SpaceExplorer.Models;

namespace SpaceExplorer.Services;

public interface INasaApodService
{
    // Picture for a specific calendar day (or "today" if null).
    Task<ApodPicture> GetByDateAsync(DateOnly? date, CancellationToken ct);

    // N random APOD entries. NASA caps `count` at 100 server-side;
    // we clamp to a friendlier 20 in the implementation.
    Task<IReadOnlyList<ApodPicture>> GetRandomAsync(int count, CancellationToken ct);

    // Inclusive date range, oldest → newest.
    Task<IReadOnlyList<ApodPicture>> GetRangeAsync(DateOnly start, DateOnly end, CancellationToken ct);
}
