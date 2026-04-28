Day 23 - Microsoft's web framework, in a Docker container, talking to NASA.


🚀TechFromZero Series - DotNetFromZero


This isn't a Hello World. It's a real space explorer:
📐 Razor Pages → Minimal API → NASA APOD → EF Core → SQLite


🔗 The full code (with step-by-step commits you can follow):
https://github.com/dev48v/dotnet-from-zero


🧱 What I built (step by step):
1️⃣ Scaffolded an ASP.NET Core 8 Razor Pages project — no host install, ran everything inside the .NET 8 SDK Docker image.

2️⃣ Wrote a multi-stage Dockerfile (sdk → aspnet runtime) so the production image lands at ~220 MB, plus a Dockerfile.dev with `dotnet watch` for hot reload behind compose profiles.

3️⃣ Built a typed `NasaApodService` using `IHttpClientFactory` so HttpClient is pooled — no more "new HttpClient() per request" socket leaks.

4️⃣ Added Minimal API endpoints under /api/apod/* (today, random, range), each one a `MapGet` lambda with the service injected straight into the parameter list.

5️⃣ Wired up EF Core 8 + SQLite. Single-file database, unique index on date, schema applied on startup with `EnsureCreated()`.

6️⃣ Built the Favourites CRUD layer — POST is idempotent by design so the same picture can't be saved twice.

7️⃣ Built four Razor Pages — Today, Random, History, Favourites — sharing one layout with a hand-rolled dark space theme. Forms post back to handler methods; zero JavaScript.

8️⃣ Plugged in Swashbuckle so /swagger renders interactive OpenAPI docs that read from the minimal-API metadata automatically.


💡 Every file has detailed comments explaining WHY, not just what. Written for any beginner who wants to learn .NET by reading real code — with full clarity on each step.


👉 If you're a beginner learning .NET, clone it and read the commits one by one. Each commit = one concept. Each file = one lesson. Built from scratch, so nothing is hidden.


🔥 This is Day 23 of a 50-day series. A new technology every day. Follow along!


🌐 See all days: https://dev48v.infy.uk/techfromzero.php


#TechFromZero #Day23 #DotNet #LearnByDoing #OpenSource #BeginnerGuide #100DaysOfCode #CodingFromScratch
