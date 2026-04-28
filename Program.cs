// ================================================================
// STEP 1 — Project entry point.
// STEP 3 — Register HttpClient + NasaApodService for DI.
// STEP 4 — Map /api/apod/* minimal-API endpoints.
// STEP 5 — Add EF Core SQLite + apply schema on startup.
// STEP 6 — Map /api/favourites/* CRUD endpoints.
// ================================================================
using Microsoft.EntityFrameworkCore;
using SpaceExplorer.Api;
using SpaceExplorer.Data;
using SpaceExplorer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// AddHttpClient<TClient> wires up:
//   * a fresh HttpClient per request (no socket exhaustion),
//   * a configured BaseAddress + User-Agent,
//   * automatic registration of NasaApodService as INasaApodService.
// Anywhere we ask for INasaApodService, ASP.NET injects the same
// service backed by a pooled, factory-managed HttpClient.
// Single SQLite file, kept in the working directory so docker
// volumes can persist it. Connection string lives in
// appsettings.json under "DefaultConnection".
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=spaceexplorer.db"));

builder.Services.AddHttpClient<INasaApodService, NasaApodService>(client =>
{
    client.BaseAddress = new Uri("https://api.nasa.gov/");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("dotnet-from-zero/1.0");
});

var app = builder.Build();

// Create / migrate the SQLite schema before the first request. We
// use EnsureCreated() instead of Migrate() because the project
// has no migration history yet — the entity model is the source
// of truth. Swap to .Migrate() the moment the schema starts
// evolving in production.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapApodEndpoints();
app.MapFavouriteEndpoints();

app.Run();
