# Waypoint — Technical Architecture

## Repository layout

```
waypoint/
  docs/                          this planning set
  apps/
    web/                         Next.js (TypeScript) frontend
    api/                         ASP.NET Core host (Composition Root)
  src/
    Modules/
      Identity/
        Waypoint.Identity.Domain/
        Waypoint.Identity.Application/
        Waypoint.Identity.Infrastructure/
        Waypoint.Identity.Api/            (minimal API endpoint group)
      Users/  (same 4-layer split)
      Dreams/ Goals/ Actions/ Experiments/ Journal/ BusinessIdeas/
      AI/ Community/ Mentorship/ Notifications/ Achievements/
      Administration/ Audit/
    BuildingBlocks/
      Waypoint.Common/              shared kernel: entity base, result types,
                                     domain event base — NOT a dumping ground,
                                     only truly cross-module primitives
      Waypoint.Common.Infrastructure/  audit sink, outbox, EF conventions
  tests/
    Waypoint.Identity.Tests/
    Waypoint.Users.Tests/
    ... one test project per module (unit + integration split by folder)
    Waypoint.Api.IntegrationTests/  full-host WebApplicationFactory tests
  docker-compose.yml
  docker-compose.override.yml       local dev overrides (hot reload, seeded db)
  Waypoint.sln
```

## Why a modular monolith, not microservices

Team size, deployment simplicity, and transactional consistency needs (a
single Dream → Goal → Action cascade often needs to be created/edited
together) all favor one deployable unit. Module boundaries are enforced at
compile time (each module's `Domain`/`Application` layers reference only
`Waypoint.Common`, never another module's project) so that if a module ever
needs independent scaling or deployment, it can be lifted out — the
boundary already exists, only the deployment topology changes.

## Layering per module (Clean Architecture)

```
Domain          — entities, value objects, domain events. No framework refs.
Application     — use cases (CQRS via MediatR: Commands/Queries + Handlers),
                  FluentValidation validators, port interfaces
                  (e.g. IDreamRepository, IAiService) — no EF, no ASP.NET.
Infrastructure  — EF Core DbContext + configurations + migrations,
                  repository implementations, external service adapters.
Api             — Minimal API endpoint group for this module, mapped into
                  the host in apps/api/Program.cs. Maps DTOs <-> commands.
                  No business logic here — thin translation only.
```

Controllers/endpoints never touch EF Core directly; they send a MediatR
command/query and return its result. This keeps business logic testable
without spinning up ASP.NET or a database (Domain/Application unit tests
run in-memory, fast).

## Backend stack

- **.NET 10**, ASP.NET Core Minimal APIs (not MVC controllers — less
  ceremony, same testability via MediatR handlers).
- **EF Core 9/10** + Npgsql provider, one `DbContext` per module.
- **ASP.NET Core Identity** for Identity module (cookie auth, not JWT —
  avoids token-in-localStorage XSS exposure for a first-party SPA/BFF setup).
- **MediatR** for in-process command/query/notification dispatch between
  and within modules.
- **FluentValidation** for all command/query input validation, run as a
  MediatR pipeline behavior (validation happens before the handler, uniform
  error shape everywhere).
- **Serilog** for structured logging, sinks to console (JSON) in
  containers; correlation/trace IDs via `ASP.NET Core`'s built-in
  `Activity`/W3C trace context.
- **Health checks**: `/health/live`, `/health/ready` (DB connectivity,
  dependent services) via `Microsoft.Extensions.Diagnostics.HealthChecks`.

## Frontend stack

- **Next.js (App Router) + TypeScript**, React Server Components for
  static/marketing pages, client components for interactive app-shell
  screens.
- **Tailwind CSS** using the tokens in
  [06-design-system.md](06-design-system.md).
- Accessible component primitives via **Radix UI** (unstyled, WCAG-correct
  behavior: focus trap, roving tabindex, ARIA) styled with Tailwind — not a
  heavy pre-styled component kit, so the design system in §06 stays the
  single source of visual truth.
- Data fetching: Next.js server actions / route handlers proxy to the
  ASP.NET Core API (BFF-lite pattern) so the session cookie stays
  first-party and HttpOnly; the browser never talks to a cross-origin API
  directly.
- Forms: `react-hook-form` + `zod`, schema shared conceptually with backend
  FluentValidation rules (kept in sync by contract tests, not by codegen in
  Phase 1).

## AI architecture (built Phase 6, designed now)

```csharp
public interface IAiService
{
    Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken ct);
}

public sealed record AiRequest(
    string PromptTemplateKey,     // versioned template id, e.g. "dream-direction.v3"
    IReadOnlyDictionary<string, string> Variables,
    string UserId,
    string ConversationId,
    int MaxOutputTokens);

public sealed record AiResponse(
    string Content,
    int InputTokens,
    int OutputTokens,
    string ModelId,
    bool WasModerationFlagged);
```

- Concrete adapters (`AnthropicAiService`, `OpenAiService`,
  `AzureOpenAiService`, `LocalModelAiService`) live in
  `Waypoint.AI.Infrastructure` and are selected via DI/config
  (`AiProvider: "Anthropic"`), so no application code references a specific
  vendor SDK.
- **Prompt templates are data, not code** — stored as versioned records
  (`prompt_templates` table: `Key, Version, SystemPrompt, UserPromptFormat,
  IsActive`), never Simon-Squibb-derived text; all templates are original
  content authored for Waypoint and reviewed for copyright/brand-safety
  before activation (ties back to
  [01-product-requirements.md §9](01-product-requirements.md)).
- Every AI call is wrapped with: token budget check, per-user rate limit,
  retry-with-backoff on transient failure, moderation pass on output before
  it's shown, and a usage record written for `/admin/ai-usage`.
- API keys live in environment variables / a secrets manager
  (`appsettings` never contains a real key; `appsettings.Development.json`
  is git-ignored), and are read only by the Infrastructure adapter — never
  serialized into any DTO returned to the frontend.
- Prompt-injection mitigation: user-supplied content is always interpolated
  into the *user* message slot of a template, never concatenated into the
  system prompt; the system prompt explicitly instructs the model to treat
  user content as data, not instructions, and output is schema-validated
  before use (e.g. Dream Direction generation expects a fixed JSON shape —
  anything else is rejected and retried once, then surfaced as an error).

## Infrastructure

- **Docker Compose** services: `api`, `web`, `postgres`, (later) `redis` for
  distributed rate limiting/session cache once horizontally scaled.
  Environment-based config via `.env` (git-ignored) + `.env.example`
  (committed, no secrets).
- **CI-ready**: build both apps, run `dotnet test`, `npm run lint`,
  `npm run typecheck`, `npm run build`; migrations applied against a
  throwaway Postgres container in CI before integration tests run.
- **Observability**: structured JSON logs, `/health/*` endpoints, and an
  `IAppMetrics` abstraction (Phase 9+) so a concrete provider (e.g.
  OpenTelemetry exporter) can be added without touching call sites.

## Security baseline (enforced from Phase 1)

- Passwords hashed via ASP.NET Core Identity's PBKDF2 implementation
  (never custom crypto).
- Cookie auth: `HttpOnly`, `Secure`, `SameSite=Strict`, short sliding
  expiration + refresh.
- CSRF: double-submit cookie token required on all mutating requests
  (cookie auth is CSRF-vulnerable by default; this closes that gap).
- Rate limiting via ASP.NET Core's built-in `Microsoft.AspNetCore.RateLimiting`.
- All input validated server-side via FluentValidation regardless of
  client-side validation — client validation is UX only, never trusted.
- Authorization via policy-based RBAC (`[Authorize(Policy = "...")]` on
  endpoint groups); admin surface is a fully separate policy set, never
  reachable by a regular user role even if a route is guessed.
- Secrets via environment variables / user-secrets locally, a secrets
  manager in any real deployment — never committed, never logged.

## Phased plan

See [09-phased-plan.md](09-phased-plan.md) for the full 12-phase rollout;
this document's structure (modules, layering, AI abstraction) is designed to
support all 12 phases without rearchitecting later phases.
