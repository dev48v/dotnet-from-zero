// ================================================================
// STEP 3 — Concrete service that calls api.nasa.gov.
// The constructor takes an HttpClient that ASP.NET's
// `IHttpClientFactory` already configured with `BaseAddress` and
// a User-Agent. Why factory? Without it, a `new HttpClient()` per
// request leaks sockets — a famous .NET footgun.
//
// The API key comes from configuration (env var NASA_API_KEY,
// see docker-compose.yml). DEMO_KEY is the public fallback.
// ================================================================
using System.Globalization;
using System.Net.Http.Json;
using SpaceExplorer.Models;

namespace SpaceExplorer.Services;

public class NasaApodService : INasaApodService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public NasaApodService(HttpClient http, IConfiguration config)
    {
        _http = http;
        // Configuration order in .NET: appsettings.json →
        // appsettings.{Environment}.json → environment variables
        // → command-line args. So an env var always wins, which
        // is exactly what we want for secrets in Docker.
        _apiKey = config["NASA_API_KEY"] ?? "DEMO_KEY";
    }

    public async Task<ApodPicture> GetByDateAsync(DateOnly? date, CancellationToken ct)
    {
        // If the caller did not pin a date, NASA returns "today"
        // in US/Eastern time. We just omit the parameter.
        var query = $"?api_key={_apiKey}";
        if (date is { } d) query += $"&date={d:yyyy-MM-dd}";

        var picture = await _http.GetFromJsonAsync<ApodPicture>(
            "planetary/apod" + query, ct);

        // NASA never sends 200 with a null body for this endpoint,
        // but the compiler can't know that — guard explicitly.
        return picture ?? throw new InvalidOperationException(
            "NASA returned an empty APOD response.");
    }

    public async Task<IReadOnlyList<ApodPicture>> GetRandomAsync(int count, CancellationToken ct)
    {
        // Defensive clamp. NASA accepts 1..100 but anything above
        // 20 noticeably slows the page, and DEMO_KEY rate-limits
        // big bursts.
        count = Math.Clamp(count, 1, 20);

        var result = await _http.GetFromJsonAsync<List<ApodPicture>>(
            $"planetary/apod?api_key={_apiKey}&count={count}", ct);

        return result ?? Array.Empty<ApodPicture>();
    }

    public async Task<IReadOnlyList<ApodPicture>> GetRangeAsync(DateOnly start, DateOnly end, CancellationToken ct)
    {
        // NASA rejects ranges where start > end. Swap silently so
        // the caller doesn't have to think about ordering.
        if (start > end) (start, end) = (end, start);

        var url = string.Format(
            CultureInfo.InvariantCulture,
            "planetary/apod?api_key={0}&start_date={1:yyyy-MM-dd}&end_date={2:yyyy-MM-dd}",
            _apiKey, start, end);

        var result = await _http.GetFromJsonAsync<List<ApodPicture>>(url, ct);
        return result ?? Array.Empty<ApodPicture>();
    }
}
