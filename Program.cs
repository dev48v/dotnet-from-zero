// ================================================================
// STEP 1 — Project entry point.
// This is the very first file ASP.NET Core executes when the app
// starts. We register the services we need (Razor Pages today,
// more in later steps) and then describe the HTTP pipeline that
// every request flows through.
// ================================================================

// WebApplication.CreateBuilder bootstraps configuration, logging,
// and dependency injection in one line. Anything we add to
// `builder.Services` becomes available via constructor injection
// in pages, services, and minimal API handlers.
var builder = WebApplication.CreateBuilder(args);

// Razor Pages = HTML templates with inline C#. Beginner friendly,
// no JavaScript framework required. We add Blazor / Minimal API
// support in later steps; today we just want a page to render.
builder.Services.AddRazorPages();

var app = builder.Build();

// Production-only protections. In Development we want raw error
// pages so we can see stack traces; in Production we hide them
// behind /Error and turn on HSTS to force HTTPS for one month.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Order matters in the pipeline:
//   Static files first  → CSS / images skip routing entirely.
//   Routing             → match the URL to a page or endpoint.
//   Authorization       → enforce [Authorize] (none yet, but free).
//   MapRazorPages       → hand matched routes to Razor Pages.
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
