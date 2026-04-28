# dotnet-from-zero — Space Explorer

Day 23 of the **TechFromZero** series.
A working **C# / .NET 8** web app that browses NASA's *Astronomy Picture of the Day* and saves favourites to a local SQLite database. Built from scratch with **Razor Pages** for the UI and **Minimal APIs + EF Core** for the back end. Docker-only — no `dotnet` install required on your host.

> **Live:** https://dotnet-from-zero.onrender.com (Render free tier — first request after 15 min idle takes ~30s to wake up).

> Companion repos: [react-from-zero](https://github.com/dev48v/react-from-zero), [nodejs-from-zero](https://github.com/dev48v/nodejs-from-zero), [django-from-zero](https://github.com/dev48v/django-from-zero), [laravel-from-zero](https://github.com/dev48v/laravel-from-zero), [vue-from-zero](https://github.com/dev48v/vue-from-zero)

---

## Quick start

```bash
git clone https://github.com/dev48v/dotnet-from-zero.git
cd dotnet-from-zero

# Optional: get a free NASA key (DEMO_KEY works but is rate-limited)
cp .env.example .env
# edit .env → NASA_API_KEY=your_key_here

# Hot-reload dev mode (Razor + .cs files reload on save)
docker compose --profile dev up

# Or production-style (multi-stage build, ~220 MB image)
docker compose --profile prod up --build
```

Then open:

| URL                                  | What it shows                       |
|--------------------------------------|-------------------------------------|
| http://localhost:8080/               | Today's APOD (or pick any date)     |
| http://localhost:8080/Random         | Gallery of N random APODs           |
| http://localhost:8080/History        | Date range browser                  |
| http://localhost:8080/Favourites     | Your saved bookmarks (SQLite)       |
| http://localhost:8080/swagger        | Interactive API docs                |

API surface:

```
GET    /api/apod/today?date=YYYY-MM-DD
GET    /api/apod/random?count=5
GET    /api/apod/range?start=YYYY-MM-DD&end=YYYY-MM-DD
GET    /api/favourites
POST   /api/favourites           {date,title,url,mediaType}
DELETE /api/favourites/{id}
```

---

## Tech stack

| Layer        | Choice                                              | Why |
|--------------|-----------------------------------------------------|-----|
| Runtime      | .NET 8 (LTS)                                        | Latest LTS through Nov 2026 |
| Web          | ASP.NET Core — Razor Pages + Minimal APIs           | Single project, two surfaces |
| HTTP client  | `IHttpClientFactory`-managed `HttpClient`           | No socket leaks |
| ORM          | EF Core 8 + SQLite                                  | Single-file DB, zero install |
| Docs         | Swashbuckle (Swagger UI)                            | Auto-generated from endpoint metadata |
| Container    | Multi-stage Dockerfile (sdk → aspnet runtime)        | ~220 MB final image |

---

## Project layout

```
dotnet-from-zero/
├── Api/                   ← Minimal API endpoint groups
│   ├── ApodEndpoints.cs
│   └── FavouriteEndpoints.cs
├── Data/
│   └── AppDbContext.cs    ← EF Core DbContext
├── Models/
│   ├── ApodPicture.cs     ← NASA DTO
│   └── Favourite.cs       ← SQLite entity
├── Pages/                 ← Razor Pages (UI)
│   ├── Index.cshtml(.cs)        Today
│   ├── Random.cshtml(.cs)       Gallery
│   ├── History.cshtml(.cs)      Date range
│   ├── Favourites.cshtml(.cs)   Saved
│   └── Shared/_Layout.cshtml    Navbar / theme
├── Services/
│   ├── INasaApodService.cs
│   └── NasaApodService.cs
├── wwwroot/css/site.css   ← Dark space theme (orange accent)
├── Program.cs             ← Entry point + DI wiring
├── Dockerfile             ← Multi-stage prod image
├── Dockerfile.dev         ← `dotnet watch` hot-reload image
├── docker-compose.yml     ← dev + prod profiles
└── SpaceExplorer.csproj
```

---

## Step-by-step build guide

Every commit on `main` is one concept — clone, run `git log --oneline`, and read the code in order.

### Step 1 — Scaffold the Razor Pages project
`dotnet new webapp` produces a working app with a navbar, a home page, and a Privacy page out of the box. We add a `.gitignore` so `bin/`, `obj/`, and the runtime SQLite file never end up in version control.

### Step 2 — Multi-stage Dockerfile + dev/prod compose profiles
Two Dockerfiles: `Dockerfile.dev` ships the full SDK and runs `dotnet watch run` for hot reload; `Dockerfile` builds a `dotnet publish` artifact and copies it into the slim ASP.NET runtime image (~220 MB). `docker-compose.yml` exposes both behind `--profile dev` and `--profile prod`. Source is bind-mounted in dev, baked in for prod.

### Step 3 — NASA APOD service
A typed `HttpClient` registered via `AddHttpClient<INasaApodService, NasaApodService>(...)` so ASP.NET pools the underlying sockets for you (the famous "don't `new` an HttpClient" problem solved). `ApodPicture` is an immutable `record` with `[JsonPropertyName]` attributes that map NASA's snake_case JSON to PascalCase C# properties.

### Step 4 — Minimal API endpoints `/api/apod/*`
Three endpoints — `today`, `random`, `range`. Each is a `MapGet(...)` lambda; ASP.NET injects the service straight into the lambda's parameter list. Ranges are capped at 30 days so a typo can't burn the whole rate-limit window.

### Step 5 — EF Core SQLite + Favourite entity
`AppDbContext` exposes a `DbSet<Favourite>`; `OnModelCreating` adds a unique index on `Date` so the same picture can't be bookmarked twice. `app.Database.EnsureCreated()` runs once on startup so a fresh checkout has a working DB without any `dotnet ef` tooling.

### Step 6 — Favourites CRUD endpoints
`GET /api/favourites`, `POST /api/favourites`, `DELETE /api/favourites/{id}`. The POST handler is idempotent: if the date is already saved, it returns the existing row with **200 OK** rather than failing on the unique-index constraint.

### Step 7 — Razor Pages UI
Four pages — Today, Random, History, Favourites — share a `_Layout.cshtml` with a sticky top bar and a dark space theme (orange accent `#f97316`). Forms use the standard Razor Pages handler convention (`asp-page-handler="Save"`, `asp-page-handler="Delete"`) so saving / removing favourites works without a single line of JavaScript.

### Step 8 — Swagger UI
`Swashbuckle.AspNetCore` reads metadata from the minimal API endpoints and renders an interactive documentation page at `/swagger`. Try-it-out works against the live container.

---

## Common commands

```bash
docker compose --profile dev up           # Hot reload, source mounted
docker compose --profile dev down         # Stop and remove containers
docker compose --profile prod up --build  # Build + run production image

# Run a one-off command in the SDK image (no host install needed)
docker run --rm -v "$PWD":/app -w /app mcr.microsoft.com/dotnet/sdk:8.0 dotnet build
docker run --rm -v "$PWD":/app -w /app mcr.microsoft.com/dotnet/sdk:8.0 dotnet test

# Inspect the SQLite favourites
docker run --rm -v "$PWD":/data -w /data nouchka/sqlite3 \
  sqlite3 spaceexplorer.db "SELECT id, date, title FROM Favourites;"
```

---

## License

MIT.
