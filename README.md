# Waypoint

The operating system for turning dreams into action. An original product —
see [docs/01-product-requirements.md](docs/01-product-requirements.md) for
the brand, scope, and copyright/brand-safety guardrails this project
follows.

## What's here (Phase 1 — Foundation)

- **`apps/web`** — Next.js/TypeScript frontend: landing page, register/login,
  an auth-gated app shell, and profile settings.
- **`apps/api`** — ASP.NET Core host (composition root) wiring together the
  modular monolith.
- **`src/Modules/{Identity,Users,Audit}`** — the three modules built in
  Phase 1, each split into Domain/Application/Infrastructure/Api per
  [docs/07-technical-architecture.md](docs/07-technical-architecture.md).
- **`src/BuildingBlocks/Waypoint.Common`** — shared kernel (Entity base,
  cross-module integration events, audit port, validation pipeline).
- **`tests/`** — unit tests (Identity, Users) and an end-to-end integration
  test suite (`Waypoint.Api.IntegrationTests`) that spins up a real Postgres
  via Testcontainers.
- **`docs/`** — the full planning set: PRD, user journey/IA, domain model,
  database design, API contract, design system, technical architecture,
  phased plan.

Everything past Phase 1 (Dream Discovery, Goals & Actions, Experiment Lab,
Business Builder, Waypoint Coach, Community/Mentorship, Admin) is designed
in `docs/` but not yet built — see
[docs/09-phased-plan.md](docs/09-phased-plan.md).

## Running it locally

### Prerequisites
- .NET 9 SDK (`dotnet --version`)
- Node.js 20+ (`node --version`)
- Docker (for Postgres, and for the integration test suite)

### Backend
```bash
docker compose up -d postgres
cd apps/api/Waypoint.Api
dotnet run
```
The API listens on `http://localhost:5080`. Migrations run automatically on
startup in Development (`Waypoint:AutoMigrate` in `appsettings.json`).

### Frontend
```bash
cd apps/web
cp .env.example .env.local   # API_BASE_URL defaults to http://localhost:5080
npm install
npm run dev
```
The app runs on `http://localhost:3030` (configured in `.claude/launch.json`
for this environment; use `npm run dev` directly elsewhere, default port 3000).

### Everything via Docker Compose
```bash
cp .env.example .env
docker compose up --build
```

## Testing

```bash
# Unit tests — no external dependencies
dotnet test tests/Waypoint.Identity.Tests
dotnet test tests/Waypoint.Users.Tests

# Integration tests — requires Docker (spins up a throwaway Postgres)
dotnet test tests/Waypoint.Api.IntegrationTests
```

Frontend:
```bash
cd apps/web
npm run lint
npx tsc --noEmit
npm run build
```
