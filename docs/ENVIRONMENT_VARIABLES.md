# Environment Variables

Every environment variable Drevia reads, in production and in local dev. No real secret values
appear anywhere in this document, only variable names, purposes, and example formats.

Local dev uses `.env.example` (root) and `apps/web/.env.example`. Production (Hostinger VPS, via
`docker-compose.prod.yml`) uses `.env.production.example`. See
[HOSTINGER_DEPLOYMENT.md](HOSTINGER_DEPLOYMENT.md) for how to obtain/generate real values on the
VPS itself.

## Database

| Variable | Required | Purpose | Example | Used in |
|---|---|---|---|---|
| `POSTGRES_DB` | Yes (prod) | Postgres database name | `drevia` | `postgres` container env, and templated into the API's connection string |
| `POSTGRES_USER` | Yes (prod) | Postgres role name | `drevia` | same |
| `POSTGRES_PASSWORD` | Yes (prod) | Postgres role password | a random 32+ char string | same |
| `ConnectionStrings__Postgres` | Yes | Full Npgsql connection string the API actually connects with | `Host=postgres;Port=5432;Database=drevia;Username=drevia;Password=...` | `apps/api/Waypoint.Api/appsettings.json`, every module's `DependencyInjection.cs`. In production this is assembled automatically by `docker-compose.prod.yml` from the three `POSTGRES_*` values above, so you don't set it directly. |

## ASP.NET Data Protection

| Variable | Required | Purpose | Example | Used in |
|---|---|---|---|---|
| `Waypoint__DataProtection__KeysDirectory` | Yes outside Development | Filesystem path (backed by a persistent volume) where auth-cookie/antiforgery encryption keys are stored. Without this set, the app throws on startup outside Development rather than silently using an ephemeral key ring that would invalidate every session on every restart. | `/keys` | `apps/api/Waypoint.Api/Program.cs`. Hardcoded to `/keys` in `docker-compose.prod.yml`, mapped to the `drevia_dataprotection_keys` named volume — not something you set per-deploy. |

## AI (Drevia Coach)

| Variable | Required | Purpose | Example | Used in |
|---|---|---|---|---|
| `ANTHROPIC_API_KEY` | No | Anthropic API key for Drevia Coach. Without it, Coach fails gracefully (a clear "not configured yet" error), the rest of the app is unaffected. Server-side only, never sent to the browser. | `sk-ant-...` | `src/Modules/AI/Waypoint.AI.Infrastructure/AnthropicAiService.cs` |
| `Waypoint__AI__Model` | No | Overrides the Claude model Coach uses | `claude-sonnet-4-5-20250929` (current default) | same file |

## Email (SMTP)

| Variable | Required | Purpose | Example | Used in |
|---|---|---|---|---|
| `Email__Smtp__Host` | No (group) | SMTP relay hostname. Left empty, the API logs emails to its own container output instead of sending them, a safe fallback, not a broken state. | `smtp.yourprovider.com` | `src/Modules/Identity/Waypoint.Identity.Infrastructure/SmtpEmailSender.cs` / `DependencyInjection.cs` |
| `Email__Smtp__Port` | No | SMTP port | `587` | same |
| `Email__Smtp__EnableSsl` | No | Whether to use TLS | `true` | same |
| `Email__Smtp__Username` | No | SMTP auth username | provider-specific | same |
| `Email__Smtp__Password` | No | SMTP auth password | provider-specific secret | same |
| `Email__Smtp__FromAddress` | No | "From" address on outgoing mail | `no-reply@drevia.net` | same |
| `Email__Smtp__FromName` | No | "From" display name | `Drevia` | same |

In `docker-compose.prod.yml` these are set from `SMTP_HOST`, `SMTP_PORT`, `SMTP_ENABLE_SSL`,
`SMTP_USERNAME`, `SMTP_PASSWORD`, `SMTP_FROM_ADDRESS`, `SMTP_FROM_NAME` in your `.env` file.

## Application / URLs / CORS

| Variable | Required | Purpose | Example | Used in |
|---|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Yes | ASP.NET Core hosting environment. Controls HSTS/HTTPS redirection, log formatting, and the Data Protection fail-fast check above. | `Production` | Hardcoded to `Production` in `docker-compose.prod.yml` |
| `Waypoint__WebAppBaseUrl` | Yes | The public URL the web app is served at. Used as the CORS allowlist origin (the API only accepts browser requests from this exact origin) and as the base URL for links in transactional emails (verify-email, reset-password). | `https://app.drevia.net` | `apps/api/Waypoint.Api/Program.cs`, `IdentityLinkOptions` |
| `WEB_APP_BASE_URL` | Yes (prod) | Same value as above, set once in `.env` and used to derive both `Waypoint__WebAppBaseUrl` (api) and `NEXT_PUBLIC_SITE_URL` (web build) in `docker-compose.prod.yml` | `https://app.drevia.net` | `docker-compose.prod.yml` |
| `NEXT_PUBLIC_SITE_URL` | Yes (prod) | The web app's own public URL. **Baked into the client JS bundle at build time**, not read at runtime, since it has the `NEXT_PUBLIC_` prefix. Used for `metadataBase` (Open Graph/Twitter absolute URLs), `robots.ts`, `sitemap.ts`. | `https://app.drevia.net` | `apps/web/src/app/layout.tsx`, `robots.ts`, `sitemap.ts` |
| `API_BASE_URL` | Yes | Where the Next.js server (not the browser) reaches the API. Internal-only, never sent to the browser. | `http://api:8080` (production, internal Docker network) / `http://localhost:5080` (local dev) | `apps/web/src/lib/api-config.ts`, `proxy.ts` |
| `Waypoint__AutoMigrate` | No | Whether the API applies pending EF Core migrations automatically on startup | `true` | `apps/api/Waypoint.Api/Program.cs` |
| `Waypoint__AdminEmails__0`, `__1`, ... | No | Email addresses granted the Admin role on first login. ASP.NET Core's array-binding convention (index per variable) — see `RoleSeeder.cs`. | `owner@drevia.net` | `src/Modules/Identity/Waypoint.Identity.Infrastructure/RoleSeeder.cs` |
| `Waypoint__ModeratorEmails__0`, ... | No | Same, for the least-privilege Moderator role (moderation queue + mentor verification only) | `mod@drevia.net` | same |
| `Waypoint__RateLimits__Auth` / `Api` / `Ai` | No | Per-minute request limits (auth endpoints, general API, AI endpoints). Sensible defaults already in code (10/100/20). | `10` | `apps/api/Waypoint.Api/Program.cs` |

## Authentication

This app uses cookie-based session authentication (ASP.NET Core Identity), not JWT — there is no
JWT signing key or token-related environment variable to configure. The two cookies involved
(`drevia.auth`, session; `drevia.csrf`, CSRF double-submit) have fixed names set in code, not
environment-configurable, since they aren't secrets and don't need to vary by deployment.

## Security

Every other secret this application currently needs is covered above. There is no separate
generic "secret key" — session/antiforgery protection comes from the Data Protection key ring
described above, not a manually-configured signing secret.

## Not applicable

- **JWT** — not used (see Authentication above).
- **Redis / distributed cache** — not used. The one in-memory cache (`AddMemoryCache`) is
  single-instance by design; see the comment above it in `Program.cs`.
