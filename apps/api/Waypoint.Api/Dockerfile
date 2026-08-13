# Multi-stage build for the Waypoint API (ASP.NET Core modular monolith).
#
# Build context is the repository root (see docker-compose.yml's
# `build.context: .`) because this solution spans many module projects under
# src/Modules/**, all referenced by apps/api/Waypoint.Api/Waypoint.Api.csproj
# — a Dockerfile scoped to this project's own directory couldn't see them.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Waypoint.sln .
COPY src/ src/
COPY apps/api/Waypoint.Api/ apps/api/Waypoint.Api/

RUN dotnet publish apps/api/Waypoint.Api/Waypoint.Api.csproj -c Release -o /app --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# mcr.microsoft.com/dotnet/aspnet:9.0 is Debian-based and doesn't ship curl
# by default — installed here specifically so HEALTHCHECK below can make a
# real HTTP call without pulling in a heavier tool or writing a throwaway
# .NET program just to probe one endpoint.
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Run as a non-root user rather than the image's default root — standard
# container hardening. Costs nothing here: the app binds an unprivileged
# port and doesn't need to write anywhere outside /app.
RUN groupadd --system waypoint && useradd --system --gid waypoint waypoint
COPY --from=build /app .

# Pre-create the Data Protection key directory and own it as the app user before the volume is
# ever mounted here. A named Docker volume that's empty on first use copies whatever the image
# already has at that path (content and ownership) into the volume — so this is what makes
# /keys writable by a non-root container once docker-compose.prod.yml mounts a persistent volume
# at this same path. See Program.cs's Waypoint:DataProtection:KeysDirectory wiring.
RUN mkdir -p /keys && chown -R waypoint:waypoint /app /keys
USER waypoint

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Reuses the app's own /health/ready endpoint (checks real Postgres
# connectivity, see docs/07-technical-architecture.md) rather than a
# synthetic check, so container health genuinely reflects whether this
# instance can serve real requests.
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl -fsS http://localhost:8080/health/ready || exit 1

ENTRYPOINT ["dotnet", "Waypoint.Api.dll"]
