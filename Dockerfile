# ================================================================
# STEP 2 — Multi-stage Dockerfile.
# Stage 1 (sdk):     compiles the app into ./out using .NET 8 SDK.
# Stage 2 (runtime): copies ./out into the slim ASP.NET runtime
#                    image — no compiler shipped to production.
# Result: ~220 MB final image instead of ~800 MB SDK image.
# ================================================================

# ---------- Stage 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the .csproj first and restore. Docker caches this layer, so
# `docker build` only re-downloads NuGet packages when the project
# file actually changes — not on every code edit.
COPY SpaceExplorer.csproj ./
RUN dotnet restore

# Now copy the rest of the source and publish a release build.
# /app/publish ends up with a self-contained set of DLLs ready to
# run under the ASP.NET runtime image.
COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---------- Stage 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Kestrel listens on port 8080 inside the container. We map it to
# the host port in docker-compose.yml so we don't need root to
# bind to 80, and so we can run multiple apps side-by-side.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "SpaceExplorer.dll"]
