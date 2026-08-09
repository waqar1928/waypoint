# Phase 9 — Security Hardening: First Formal Pass

Date: 2026-08-09. Scope: full OWASP Top 10 (2021) pass plus rate-limit tuning across
everything built in Phases 1–8 (11 backend modules, the Next.js BFF, and the admin
surface). Each category below states what was reviewed, what was found, and what
changed. No phase/category is marked clean on a claim alone — see "Verification" at
the end for how each fix was confirmed.

## A01 — Broken Access Control: PASS, no fixes needed

- Every endpoint group requires authentication (`RequireAuthorization()`); every
  admin group requires the `Admin` policy. No `AllowAnonymous` overrides exist
  anywhere in the codebase.
- Audited all 16 handlers behind resource-ID route parameters (Actions, Goals,
  Experiments, Community, Mentorship, AI modules) for IDOR risk. Every one that
  should check ownership does — comparing the resource's owning `DreamId`/`UserId`
  against the current user and throwing `NotFoundException` on mismatch (never a
  403, which would leak the resource's existence to a non-owner).
- `GetHelpRequestResponsesQuery` has no ownership check, but this is confirmed
  intentional: `HelpRequest` has no privacy/visibility concept anywhere in the
  domain model — it's designed as a public mentor board, consistent with
  `GetHelpRequestsQuery` and `RespondToHelpRequestCommand` also being open to any
  authenticated user.
- No mass-assignment risk: every command is a narrow, explicit DTO (e.g.
  `UpdateMyProfileCommand(string DisplayName, string? Bio, string TimeZone)`),
  never bound directly to a domain entity, so there's no way to smuggle a
  privileged field (like an admin flag) through a normal update endpoint.

## A02 — Cryptographic Failures: 1 real gap found and fixed

- Password hashing, cookie flags (`HttpOnly`, `SameSite=Strict`,
  `Secure=SameAsRequest`), and the Anthropic API key handling (env var only,
  never logged, never echoed in error responses) are all correct.
- `GlobalExceptionHandler`'s fallback case never includes exception details or
  stack traces in the response body, in any environment — more conservative than
  ASP.NET Core's own default developer exception page behavior.
- **Fixed:** the API had no `UseHttpsRedirection()`/`UseHsts()` anywhere.
  Added both, gated behind `!app.Environment.IsDevelopment()` so local dev (which
  runs plain HTTP on `localhost:5080` with no dev cert configured) is unaffected.
  A production deployment terminating TLS at Kestrel now gets both; one
  terminating TLS at a reverse proxy/load balancer gets a harmless no-op.

## A03 — Injection: PASS, no fixes needed

- Zero raw SQL anywhere (`FromSqlRaw`/`ExecuteSqlRaw`/interpolated variants) —
  every query goes through EF Core's parameterized LINQ.
- Zero `dangerouslySetInnerHTML` in the frontend — React's default escaping
  covers all user-generated content (post bodies, comments, journal entries,
  etc.), and there's nowhere that bypasses it.
- Swept every command for a missing FluentValidation validator; the handful that
  don't have one take only a bare `Guid` (route-constrained, type-safe) or plain
  `bool` parameters — no free-text injection surface. Free-text fields (post
  body, journal entry, etc.) all have `MaximumLength` validators.

## A04/A05 — Insecure Design / Security Misconfiguration: 1 real gap found and fixed

- CORS is scoped to a single configured origin (`Waypoint:WebAppBaseUrl`), not a
  wildcard — correct.
- The CSRF double-submit setup (`AntiforgeryValidationMiddleware`) correctly
  covers every mutating `/api/v1` call including pre-auth ones (register/login),
  with the only exclusion being the token-issuing endpoint itself.
- No Swagger/OpenAPI UI exposed anywhere — nothing to leak.
- `UseSerilogRequestLogging()` uses default options, which never capture request
  or response bodies — no risk of passwords/tokens landing in logs via this path.
- **Fixed:** neither the API nor the Next.js app sent any standard security
  response headers (`X-Content-Type-Options`, `X-Frame-Options`,
  `Referrer-Policy`, `Permissions-Policy`). Added all four unconditionally to
  both. Also added a production-only `Content-Security-Policy` to the frontend
  (`default-src 'self'`, `frame-ancestors 'none'`, `object-src 'none'`, etc.) —
  kept dev-only-conditional because Next.js's Turbopack Fast Refresh needs
  `unsafe-eval`/`unsafe-inline`, which would make a dev-mode CSP either a no-op
  or a broken dev loop.

## A06 — Vulnerable/Outdated Components: PASS, no fixes needed

- `dotnet list package --vulnerable --include-transitive` across all 60 backend
  projects: zero vulnerable packages.
- `npm audit` on the frontend: zero vulnerabilities.

## A07 — Authentication Failures: PASS, one decision surfaced (kept as-is)

- Password policy (min length 10, uppercase + digit required), account lockout
  (5 failed attempts → 15 min lockout via `SignInManager.PasswordSignInAsync(...,
  lockoutOnFailure: true)`), and the `"auth"` rate-limit policy (10/min/IP) are
  all sound and layered correctly (network-level IP throttle plus
  per-account lockout).
- No user-enumeration surface: login returns the same generic "Incorrect email
  or password" for both a nonexistent account and a wrong password;
  forgot-password always returns success regardless of whether the email exists
  (explicitly documented in code as a deliberate choice).
- Password reset uses ASP.NET Core Identity's real token machinery
  (`UserManager.GeneratePasswordResetTokenAsync`/`ResetPasswordAsync`) — not a
  hand-rolled mechanism. Logout calls `SignInManager.SignOutAsync()`, which
  properly clears the server-recognized session, not just the client cookie.
- **Surfaced, not changed:** users can log in immediately after registering,
  with no email-confirmation gate anywhere. Raised this with the user as a
  product/security tradeoff (impersonation risk via unverified email vs. login
  friction) — decision: leave as-is for now. This was the behavior already
  verified across Phases 1–8, and the current `LoggingEmailSender` is an
  explicitly-documented dev-mode placeholder (no real email delivery exists yet
  regardless), so this is best revisited as part of Phase 12 once a real
  transactional email provider is wired in.

## A08/A09 — Data Integrity & Logging/Monitoring: 1 real gap found and fixed

- The audit log itself has no write path via the HTTP API — the only exposed
  route is `GET /api/v1/admin/audit-log`; every write happens internally via the
  `IAuditSink` port called from within command handlers. Not tamperable from the
  API surface.
- No insecure deserialization risk (`System.Text.Json` throughout; no
  `BinaryFormatter` anywhere).
- **Fixed:** of Phase 8's five admin action types, three
  (`DismissReportCommand`, `RemoveReportedContentCommand`, `ResolveReportCommand`
  — all in the Community moderation queue) wrote no audit trail entry at all,
  while the other two (`LockUserCommand`/`UnlockUserCommand`,
  `UpdateMentorVerificationCommand`) did. Added `IAuditSink` calls to all three,
  matching the existing pattern exactly (`"DismissedByAdmin"`,
  `"ContentRemovedByAdmin"`, `"ResolvedByAdmin"`, entity type `"ContentReport"`).
  Now every admin action that changes another user's data leaves a trail,
  consistently.
- Noted but not changed: the AI module's output moderation is a documented,
  intentional Phase 6 placeholder (`wasModerationFlagged` only catches an
  empty/blocked response, not real content moderation) — the code comment
  already flags this as a natural upgrade point, not a hidden gap.

## Rate limit tuning

- `"auth"` (10/min/IP) and `"api"` (100/min/user) policies were reviewed against
  real usage patterns now that all 8 phases exist — e.g. the dashboard's
  `Promise.all` fan-out is ~11 parallel GET requests per load, well within the
  100/min budget (would need ~9 reloads within 60 seconds to trip it). Both left
  unchanged.
- **Fixed:** the `"ai"` policy (20/min/user, deliberately stricter since it's
  meant to gate billed Anthropic calls) was applied to the entire
  `/api/v1/ai` route group, including two routes that are plain reads with no AI
  cost (`GET /conversations`, `GET /conversations/{id}/messages`). This meant a
  single coach-page load's list-and-history fetch shared the same 20/min budget
  as real, billed conversation turns. Split the group: only
  `POST /conversations` (StartConversation, which triggers an opening-turn AI
  call) and `POST /conversations/{id}/messages` (SendMessage) stay under `"ai"`;
  the two read routes moved to `"api"`.

## Verification

- Backend: `dotnet build` clean (0 warnings, 0 errors) after every change; full
  unit test suite green (57/57) after all fixes.
- Frontend: `npm run lint`, `npx tsc --noEmit` (via `next build`), and
  `next build` all clean.
- Live-verified against the running API (real Postgres, real session cookies):
  - `curl -D -` on `/health/ready` confirmed all four new security headers
    (`X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`,
    `Permissions-Policy`) present on responses.
  - Created a test post, reported it as a second user, dismissed it as admin,
    then confirmed a `DismissedByAdmin` / `ContentReport` entry appeared in
    `GET /api/v1/admin/audit-log`, correctly attributed to the acting admin —
    previously this action left no trace at all.
  - Fired 25 rapid `GET /api/v1/ai/conversations` requests as an authenticated
    user with zero `429`s, confirming the route now sits under the 100/min
    `"api"` policy rather than the 20/min `"ai"` policy (25 requests would have
    tripped the old, incorrect scoping well before the 20th call).

## Deferred to later phases (not gaps, by design)

- Real transactional email delivery (currently `LoggingEmailSender`, dev-mode
  only) — Phase 12.
- Secrets management for the Postgres connection string and any future
  production credentials (currently a plaintext dev-only password in
  `appsettings.json`, standard/accepted for local dev) — Phase 12.
- Real AI output moderation (currently a placeholder) — a future AI-module
  iteration, not blocking.
- Email verification as a login gate — a product decision, explicitly deferred
  by the user during this pass (see A07).
