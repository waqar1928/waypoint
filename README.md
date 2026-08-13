# Drevia

Turn your dream into your next move. An original product. See
[docs/01-product-requirements.md](docs/01-product-requirements.md) for
the brand, scope, and copyright/brand-safety guardrails this project
follows.

Note: this codebase was originally built under the working name "Waypoint."
Internal namespaces, project files, database names, and similar technical
identifiers still use that name and haven't been renamed (renaming a
namespace or a database purely for cosmetic reasons isn't worth the
breaking-change risk), but the actual product is Drevia everywhere a real
user would see it.

## What's here

- **`apps/web`** — Next.js/TypeScript frontend: landing page, register/login,
  an auth-gated app shell, and profile settings.
- **`apps/api`** — ASP.NET Core host (composition root) wiring together the
  modular monolith.
- **`src/Modules/`** — one folder per module (Identity, Users, Audit, Dreams,
  Journal, Goals, Actions, Experiments, BusinessIdeas, AI, Community,
  Mentorship, Notifications), each split into
  Domain/Application/Infrastructure/Api per
  [docs/07-technical-architecture.md](docs/07-technical-architecture.md).
- **`src/BuildingBlocks/Waypoint.Common`** — shared kernel (Entity base,
  cross-module integration events, audit port, validation pipeline).
- **`tests/`** — a unit test project per module and an end-to-end
  integration test suite (`Waypoint.Api.IntegrationTests`) that spins up a
  real Postgres via Testcontainers.
- **`docs/`** — the full planning set: PRD, user journey/IA, domain model,
  database design, API contract, design system, technical architecture,
  phased plan, plus production-readiness audits.

Note: the module list above (`src/Modules/{Identity,Users,Audit}` and "Phase
1 only") in this README was out of date before this rebrand touched it.
Dream Discovery, Goals & Actions, Experiment Lab, Business Builder, Drevia
Coach, Community/Mentorship, and Admin are all built. See
[docs/09-phased-plan.md](docs/09-phased-plan.md) for the phase-by-phase
history.

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
