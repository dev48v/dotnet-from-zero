// ================================================================
// STEP 1 — Project entry point.
// STEP 3 — Register HttpClient + NasaApodService for DI.
// ================================================================
using SpaceExplorer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// AddHttpClient<TClient> wires up:
//   * a fresh HttpClient per request (no socket exhaustion),
//   * a configured BaseAddress + User-Agent,
//   * automatic registration of NasaApodService as INasaApodService.
// Anywhere we ask for INasaApodService, ASP.NET injects the same
// service backed by a pooled, factory-managed HttpClient.
builder.Services.AddHttpClient<INasaApodService, NasaApodService>(client =>
{
    client.BaseAddress = new Uri("https://api.nasa.gov/");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("dotnet-from-zero/1.0");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
