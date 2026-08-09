# Phase 12 — Production Deployment: First Formal Pass

Date: 2026-08-10. Scope: CI/CD, environment hardening, observability — the parts of
this phase that could be done and *fully verified* in this environment. Per an
explicit scope decision at the start of this phase, Dockerfiles and any live
deployment were left out rather than shipped unverified — see "What's deliberately
not done" below for exactly why and what's needed to close those gaps.

## CI/CD

Added `.github/workflows/ci.yml` — a real GitHub Actions workflow, not a stub. Two
jobs, running in parallel on every push to `main` and every PR:

- **backend**: `dotnet restore` → `dotnet build --configuration Release` →
  unit tests (`--filter "FullyQualifiedName!~IntegrationTests"`) → integration tests.
  GitHub-hosted `ubuntu-latest` runners have Docker pre-installed, so the integration
  tests' Testcontainers path (see `docs/12-testing-audit-phase11.md`) runs for real in
  CI even though it can't in this sandbox — the local-Postgres fallback added in
  Phase 11 only kicks in here as a safety net, not the primary path.
- **frontend**: `npm ci` → `npm run lint` → `npm test` (Vitest) → `npm run build`.

Every command in both jobs was run locally first, in the same configuration CI uses
(`--configuration Release` for the backend, `npm ci` not `npm install` for the
frontend) — 105/105 backend unit tests, 4/4 integration tests, 32/32 frontend tests,
clean lint, clean build, all passing before this was ever pushed.

## Environment & secrets hardening

Two real, concrete findings from this pass — not proactive scope, things the
review actually turned up:

1. **`Waypoint:AdminEmails` was in the base `appsettings.json`, not
   `appsettings.Development.json`.** It held two real local test-account emails from
   Phase 8/10 verification. Left as-is, a production deployment that forgot to set
   `Waypoint__AdminEmails` would have silently granted admin rights to those two
   specific emails the moment anyone (accidentally or otherwise) registered with them
   on the production instance — a real, if narrow, privilege-escalation path. Moved
   the dev emails into `appsettings.Development.json`; the base file now defaults to
   an empty array, so a production deployment is safe-by-default (no admins at all)
   until someone explicitly configures the allowlist. Verified: Development still
   grants the two test accounts correctly (confirmed live via `/api/v1/auth/session`
   returning `isAdmin: true`); confirmed the empty-array default compiles and the
   seeder's `foreach` is a no-op against it.

2. **`ANTHROPIC_API_KEY` was never documented anywhere** — not in `.env.example`, not
   in any doc. Without it, Waypoint Coach fails on every single request
   (`AiServiceUnavailableException`), but nothing prior to a live AI call would tell
   you it was missing. Added it to `.env.example` with a comment explaining exactly
   what breaks without it, and noted that `docker-compose.yml`'s `api` service doesn't
   currently pass it through either (see below).

Everything else audited came back correctly configured already:

- `ConnectionStrings:Postgres`, `Waypoint:WebAppBaseUrl` — both already
  environment-variable-overridable via ASP.NET Core's standard `Section__Key`
  convention (`ConnectionStrings__Postgres`, `Waypoint__WebAppBaseUrl`).
  `WebAppBaseUrl` already throws `InvalidOperationException` on startup if unset —
  correctly forces explicit configuration rather than silently defaulting to a dev
  URL in production.
- `Waypoint__AdminEmails__0`, `Waypoint__AdminEmails__1`, etc. — the array-binding
  syntax for overriding `AdminEmails` via environment variables. Verified live
  (not assumed): ran the API with `Waypoint__AdminEmails__0=envtest@example.com` set
  and confirmed the startup seeder picked it up and processed it correctly.
- `ANTHROPIC_API_KEY` — already read via `IConfiguration`, which includes environment
  variables by default; already never logged, never echoed in any error response
  (confirmed back in Phase 9's A02 audit).

## Observability: structured logging

Serilog previously wrote plain text to the console in every environment via
`appsettings.json`'s `Serilog:WriteTo` array. That's fine to read at a dev terminal
but useless to a real log aggregator (CloudWatch, Datadog, Grafana Loki, etc.), which
needs to index and query structured fields, not grep text.

Moved the console sink registration from config into `Program.cs` with an environment
check: human-readable text in Development (unchanged from before), compact JSON
(`Serilog.Formatting.Compact.CompactJsonFormatter`) everywhere else. Also added
`Environment` and `Application` properties to every log event via `Enrich.WithProperty`,
so a real aggregator can filter/group by environment without parsing the message text.
`appsettings.json`'s `Serilog:MinimumLevel`/`Override` still control levels via
`ReadFrom.Configuration` — only the console sink's *rendering* moved into code, and
the old `"WriteTo": [{ "Name": "Console" }]` entry was removed from `appsettings.json`
to avoid double-registering the sink (would have printed every line twice).

Verified both modes live, not just by reading the code: ran the compiled API directly
(bypassing `launchSettings.json`, which otherwise silently forces
`ASPNETCORE_ENVIRONMENT=Development` and would have made this impossible to test)
under `ASPNETCORE_ENVIRONMENT=Staging` and confirmed genuinely structured JSON output
line-by-line, then confirmed health checks and Phase 9's security headers still work
correctly in that mode over plain HTTP (no HTTPS redirect loop, since no HTTPS
listener is configured — exactly the "harmless no-op" behavior documented when that
middleware was added in Phase 9).

## What's deliberately not done, and why

Per an explicit scope decision made at the start of this phase (not an oversight):

- **Dockerfiles for the API.** `docker-compose.yml` already exists in this repo (from
  Phase 1) and already references `apps/api/Waypoint.Api/Dockerfile` — **which does
  not exist**. `apps/web/Dockerfile` does exist. This means `docker compose up` is
  currently broken for the `api` service. This was found during this pass, not
  introduced by it. It was deliberately not fixed here: writing a multi-stage
  ASP.NET Core Dockerfile is mechanical, but this sandbox has no Docker daemon to
  actually run `docker build` and confirm it works, and shipping an unverified
  Dockerfile would violate this project's own "no fake implementations, verify
  everything" standard. **This is the single most concrete, actionable gap left by
  this phase** — someone with Docker available needs to add
  `apps/api/Waypoint.Api/Dockerfile` (multi-stage: `mcr.microsoft.com/dotnet/sdk:9.0`
  build stage running `dotnet publish`, `mcr.microsoft.com/dotnet/aspnet:9.0` runtime
  stage, matching the layering already established in `apps/web/Dockerfile`) and
  confirm `docker compose up` actually brings up all three services successfully.
  Note also that `docker-compose.yml`'s `api` service doesn't currently pass through
  `ANTHROPIC_API_KEY` at all — add that alongside the Dockerfile fix.
- **Live deployment to any real host** (Azure, AWS, Railway, Vercel, etc.). Needs
  real hosting credentials/account access this session doesn't have. Once the
  Dockerfile above exists, the CI workflow added this phase is the natural place to
  add a `deploy` job (build + push an image, trigger a deploy) — structured as a
  separate job gated on the `backend`/`frontend` jobs succeeding.
- **A real observability backend** (APM/tracing — Application Insights, Datadog,
  Honeycomb, etc.). The structured JSON logging above is the right *shape* of data
  for one, but nothing here ships logs anywhere but stdout, and there's no
  distributed tracing (OpenTelemetry) wired in. Reasonable next step once a real
  aggregator is chosen, but speculative instrumentation with no backend to verify
  against would be exactly the kind of unverified work this project has avoided at
  every prior phase.

## Verification

- Backend: `dotnet build` clean; full unit suite 105/105; integration suite 4/4 — all
  run in `--configuration Release`, matching what CI actually runs, not just Debug.
- Frontend: `npm ci` (not `npm install`, matching CI) → lint clean → 32/32 tests →
  clean build.
- Live-verified the two behavior-affecting changes (AdminEmails default, structured
  logging) against the running API in multiple environment modes (Development,
  Staging), not just by reading the code.
- The CI workflow itself is verified by the fact that every command in it was run
  locally in the exact configuration CI uses before this was written — the workflow
  runs the same commands, so a green result on GitHub after pushing would confirm the
  environment match, not test anything new.
