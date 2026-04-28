// ================================================================
// STEP 3 — DTO that mirrors NASA's APOD JSON.
// APOD = Astronomy Picture of the Day. Sample response:
//   {
//     "date": "2024-05-12",
//     "title": "The Crab Nebula from Hubble",
//     "explanation": "...",
//     "url": "https://apod.nasa.gov/apod/image/2405/crab.jpg",
//     "hdurl": "https://apod.nasa.gov/apod/image/2405/crab_4k.jpg",
//     "media_type": "image",
//     "copyright": "NASA / ESA"
//   }
// We use a `record` (immutable, value-equality) and JSON property
// names map snake_case → PascalCase via [JsonPropertyName].
// ================================================================
using System.Text.Json.Serialization;

namespace SpaceExplorer.Models;

public record ApodPicture
{
    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("explanation")]
    public string Explanation { get; init; } = string.Empty;

    // Standard-resolution media URL. Always present.
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    // High-resolution image URL. Only set when media_type == "image".
    [JsonPropertyName("hdurl")]
    public string? HdUrl { get; init; }

    // Either "image" or "video". When "video", `url` is a YouTube
    // / Vimeo embed link, not a JPEG. The Razor page checks this
    // to decide between <img> and <iframe>.
    [JsonPropertyName("media_type")]
    public string MediaType { get; init; } = "image";

    // Optional — many older APODs are public domain and have no
    // copyright field at all, so it must be nullable.
    [JsonPropertyName("copyright")]
    public string? Copyright { get; init; }
}
