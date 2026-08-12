# Waypoint — Production Readiness Audit

Date: 2026-08-10. Scope: full repository, all 12 prior build phases, re-verified
fresh for this audit (not from memory) — full `dotnet build`/`dotnet test`
(Release config), frontend `lint`/`test`/`build`, repo-wide grep for secrets,
TODOs, mock/fake/placeholder content, and direct review of the auth, AI,
CORS, Docker, and SEO surfaces.

**Baseline confirmed working before any changes in this pass:**
`dotnet build` — 0 warnings, 0 errors. Backend unit tests — 105/105 passing.
Frontend `lint` — clean. Frontend `test` (Vitest) — 32/32 passing. No secrets
found in tracked files. Only one TODO-shaped comment in the whole codebase
(an explanatory comment, not an outstanding task).

This is not a from-scratch assessment — Phases 9–12 already did real,
verified security/performance/testing/CI passes (see
`docs/10-security-audit-phase9.md`, `11-performance-audit-phase10.md`,
`12-testing-audit-phase11.md`, `13-production-readiness-phase12.md`). This
audit does not repeat that work; it re-verifies the claims, extends coverage
into categories those passes didn't cover (legal/trust, notifications, email
delivery, SEO, Docker completeness), and tracks everything that's actually
new or still open.

Legend: **CRITICAL** = blocks real users / real legal or security exposure.
**HIGH** = must fix before a real launch. **MEDIUM** = should fix soon after
launch. **LOW** = backlog-worthy, not launch-blocking.

---

## 1. Architecture

**Current status:** Modular monolith, Clean Architecture per module
(Domain/Application/Infrastructure/Api), 12 bounded-context modules
(Identity, Users, Audit, Dreams, Journal, Goals, Actions, Experiments,
BusinessIdeas, AI, Community, Mentorship). Modules reference only
`Waypoint.Common`; cross-module reads go through published read-contract
interfaces (`IDreamSummaryProvider`, `IProfileSummaryProvider`, etc.);
cross-module writes go through MediatR integration events or shared sink
ports (`IAuditSink`, `IContentReportSink`). This boundary was verified held
consistently in the Phase 9 A01 audit.

**Gap:** No dedicated **Notifications** module. `Waypoint.Users.Domain` has
a `NotificationPreferences` entity (three boolean toggles: product updates,
coach nudges, community activity) but nothing reads it — no in-app
notification feed, no event-triggered email dispatch, nothing that actually
uses these preferences today.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| No Notifications module (delivery/inbox) | HIGH | Users have no way to be told about mentorship replies, comments, moderation outcomes, etc. except by manually checking | Build a minimal Notifications module: an in-app notification feed backed by real trigger points (new comment on your post, new response to your help request, moderation outcome, account security events), reusing the existing `NotificationPreferences` toggles to gate optional categories | This pass |
| `.NET 9` vs. the `.NET 10+` requested in this brief | LOW | This sandbox only has the .NET 9 SDK installed (`dotnet --list-sdks` confirms it); attempting a major-version migration here would be unverifiable — no SDK to build or test against | Do NOT blind-migrate. Documented here as a real backlog item for whoever has .NET 10 available to verify a build/test pass before merging | Backlog, not this pass |

---

## 2. Backend

**Current status:** ASP.NET Core minimal APIs, MediatR CQRS with
`ValidationBehavior<TRequest,TResponse>` (FluentValidation) as a pipeline
behavior — every command/query is validated before it reaches a handler.
`dotnet build` clean, 105 unit tests + 4 integration tests, all passing.

No issues found beyond what's already tracked above/below.

---

## 3. Frontend

**Current status:** Next.js App Router, React 19, TypeScript, Tailwind. BFF
proxy pattern (`proxyToApi`) so the browser never talks to the API origin
directly — cookies stay first-party/HttpOnly. `npm run lint` clean, `next
build` clean, 32 Vitest unit tests passing. No `console.log`/`console.debug`
in source. No hardcoded demo credentials found.

**Real, minor finding:** the community post-composer's visibility selector
has an option labeled "Public (coming soon)" — but the backend has
processed `Public` identically to `Community` since Phase 7 (documented
decision, not actually unimplemented). This is a UI label lying about
feature status, not a missing feature.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| "Public (coming soon)" label on a working feature | LOW | Minor user confusion; visible on every post composer interaction | Fix the label — either remove "(coming soon)" since it works, or genuinely disable the option if Public should stay unavailable for launch | This pass |

---

## 4. Database

**Current status:** Postgres + EF Core, snake_case naming convention, xmin
optimistic concurrency on every module, `AuditableEntitySaveChangesInterceptor`
for CreatedAt/UpdatedAt, soft-delete via `ISoftDeletable` where applicable.
Phase 10 already did a real index/query audit (N+1 fixed, `AsNoTracking()`
swept across ~20 read-only queries, one missing index added, xmin-concurrency
regression caught and fixed). Migrations are the only way schema changes
happen — confirmed no raw `ALTER TABLE`/manual DDL anywhere in the codebase.

No new issues found in this pass — Phase 10's audit already covered this
surface thoroughly and it re-verified clean.

---

## 5. Authentication

**Current status:** ASP.NET Core Identity, PBKDF2 password hashing
(framework default, never reinvented), HttpOnly/SameSite=Strict cookies,
account lockout (5 failed attempts → 15 min), rate limiting on auth
endpoints (10/min/IP), CSRF double-submit protection on every mutating
request. Password policy: 10+ chars, uppercase + digit required. Session:
14-day sliding cookie expiration. Logout calls `SignInManager.SignOutAsync()`
(real server-side session invalidation). Account deletion exists, gated on
password re-verification.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| **No real email delivery.** `LoggingEmailSender` just logs verification/reset emails to console | **CRITICAL** | No real user can verify their email or recover a forgotten password. This alone blocks any real launch | Build a real `IEmailSender` abstraction cleanly (already exists as a port); do NOT wire a real provider/send real email without explicit approval — that's in this task's own stop-list ("sending real emails to users", "purchasing services"). Fix in this pass: make the abstraction swap-ready and document exactly what's needed | This pass (abstraction); **user decision required for the real provider** |
| Email verification not enforced before login | HIGH | A user can register with an email they don't own and use the account immediately, including impersonating that identity in Community/Mentorship, before ever proving ownership | Enforce `RequireConfirmedAccount`-equivalent gate on login; add a resend-verification endpoint (currently doesn't exist) | This pass |
| No resend-verification endpoint | HIGH | Once a verification email is missed/expired, a user has no way to request a new one | Add `POST /api/v1/auth/resend-verification` | This pass |
| **No frontend pages for `/forgot-password`, `/reset-password`, or `/verify-email` at all** — found during this pass, not previously known | **CRITICAL** | The backend generates real links to these three routes (in the emails `RegisterUserCommandHandler`/`ForgotPasswordCommandHandler` send) and the login page has a real "Forgot password?" link to `/forgot-password` — but none of these pages exist in `apps/web/src/app`. Every one of those links 404s today. A user who forgets their password has **no way to recover their account**, and no user has ever been able to complete email verification through the UI, only by calling the API directly | Build all three pages this pass | This pass |
| Account deletion isn't audit-logged | MEDIUM | `DeleteAccountCommandHandler` performs a real, irreversible action with zero audit trail — the exact class of event this brief explicitly requires logging | Add an `IAuditSink` call before the delete completes | This pass |

---

## 6. Authorization

**Current status:** Policy-based (`RequireAuthorization("Admin")`), every
admin endpoint gated, config-seeded admin allowlist with no self-service
escalation path. Phase 9's A01 audit reviewed all 74 command/query handlers
for ownership checks and IDOR risk — every ownership-sensitive handler
checks `entity.UserId == currentUser.UserId` (or the dream-scoped
equivalent) and returns `NotFoundException` (never `Forbidden`, which would
leak existence) on mismatch. Re-verified in this pass: still holds, no
regressions.

**Gap vs. this brief's requested role model:** the brief asks for
`User / Mentor / Moderator / Admin / SuperAdmin`. The current model has
exactly one role (`Admin`) plus an implicit "everyone else" — "Mentor" is a
*profile state* (`MentorProfile.VerificationStatus`), not an authorization
role, and there's no `Moderator` or `SuperAdmin` tier at all.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| Only one real role (`Admin`); no `Moderator`/`SuperAdmin` tier | MEDIUM | Every admin today has full power (user lock, mentor verification, content removal) — no least-privilege separation. Not a vulnerability by itself, but doesn't match "production-grade" role separation | Add a `Moderator` role scoped to the moderation queue + mentor verification only, distinct from full `Admin` (user lock/unlock, system-wide). Defer `SuperAdmin` — with a single admin allowlist today, that tier has no real use case yet | This pass (Moderator); SuperAdmin backlog |
| "Mentor" isn't a real role | LOW | Working as designed — mentorship is opt-in profile data, not a privilege tier. Not actually a gap | No action | N/A |

---

## 7. Security

**Current status:** Phase 9 did a genuine OWASP Top 10 pass (see
`docs/10-security-audit-phase9.md`) — A01 (access control) clean, A02
(crypto) fixed HSTS/HTTPS-redirect gap, A03 (injection) clean (zero raw SQL,
zero `dangerouslySetInnerHTML`), A04/A05 fixed missing security headers, A06
zero vulnerable dependencies (re-confirmed in this pass: `dotnet list
package --vulnerable` and `npm audit` both still clean), A07 (auth) clean,
A08/A09 fixed an inconsistent audit-trail gap. Re-verified in this pass, all
still holds.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| CORS: `AllowAnyMethod()`/`AllowAnyHeader()` (origin IS restricted to one configured value) | MEDIUM | Origin restriction is the load-bearing control here and it's correct, but the brief explicitly flags any `AllowAny*` as needing a documented reason. Broad method/header allowance is unnecessary for a BFF that only ever sends a known, small set of methods/headers | Restrict to the actual methods/headers this API uses (`GET,POST,PUT,DELETE`, `Content-Type`, `X-CSRF-TOKEN`) | This pass |
| `.gitignore` doesn't cover `secrets.json`, `credentials*`, `*.pem`/`*.key`, `appsettings.*.local.json` | LOW | Nothing currently violates this (verified — zero secrets in tracked files), but the safety net is thinner than it should be | Broaden `.gitignore` | This pass |
| No retry/circuit-breaker on the outbound Anthropic call | MEDIUM | A transient network blip fails the whole AI turn immediately with no retry; not a security issue, a reliability one | Add a bounded retry (2 attempts, short backoff) for transient (5xx/timeout) failures only — never retry on 4xx | This pass |

---

## 8. AI

**Current status:** Provider abstraction (`IAiService`) already exists —
`AnthropicAiService` is the only class in the codebase that references
Anthropic types, confirmed via Phase 6/9 review. Prompt-injection mitigation
is real and documented: user text only ever lands in the `user` message
slot, never appended to or concatenated with the system prompt; the system
prompt is fixed, versioned, data-driven `PromptTemplate` content, and it
explicitly instructs the model to treat user content as information, not
instructions. System prompts and the API key are never returned to the
client (confirmed: `AiResponse` only carries `Content`/token counts/model
name/moderation flag). 60-second HTTP timeout, 800-token output cap per
turn, per-user rate limiting (20/min, split from plain reads in Phase 10 so
reads don't compete with the same budget as billed calls). AI usage is
tracked and surfaced in the admin panel (aggregate conversations/messages/
tokens by topic).

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| No hard per-conversation message cap | MEDIUM | Per-minute rate limiting bounds the *rate* of spend but not the *total size* of one conversation — a very long-running single conversation could still accumulate significant token cost over hours/days | Add a reasonable per-conversation message ceiling (e.g. 100 turns) that ends the conversation gracefully rather than growing unbounded | This pass |
| AI output moderation is a placeholder (empty-response check only) | MEDIUM | No real content-safety filtering on AI output today — documented as an intentional Phase 6 placeholder with a clear upgrade path (`WasModerationFlagged` already wired through the DTO) | Out of scope to fully solve this pass (needs a real moderation API/service decision) — flagged clearly in the UI copy instead (see Legal/Trust) so it's honest about what it is | Backlog; UI disclosure this pass |
| No overall AI spend cap/alert on the account | MEDIUM | Per-user rate limits exist; nothing caps total platform-wide spend if usage patterns shift | This is an Anthropic-console-side control (billing alerts/spend limits), not application code — **flagging for the user to configure on their Anthropic account**, not something I can safely do myself | User action (not app code) |
| No retry on transient failures (see Security section) | MEDIUM | Duplicate of the Security-section entry | See above | This pass |

---

## 9. API

**Current status:** Every endpoint requires authentication by default (only
`/api/v1/auth/register`, `/login`, `/forgot-password`, etc. are
intentionally open, and none use `AllowAnonymous` bypasses — confirmed zero
matches repo-wide). RFC 7807 Problem Details via `GlobalExceptionHandler`;
the unhandled-exception fallback never includes exception details or stack
traces in ANY environment (more conservative than ASP.NET Core's own
default). DTOs used consistently — no domain entities exposed directly over
the wire, confirmed via the narrow-DTO pattern used in every module.

No new issues found — this was already covered thoroughly in Phase 9's A01
and A04/A05 passes, re-verified clean.

---

## 10. Validation

**Current status:** FluentValidation via a MediatR pipeline behavior — every
command is validated before it reaches its handler, structurally impossible
to skip. Every free-text field has a `MaximumLength` rule (verified in
Phase 9's A03 pass). Zero raw SQL anywhere (parameterized EF Core LINQ
throughout). Zero `dangerouslySetInnerHTML` in the frontend (React's default
escaping covers all rendered user content).

No new issues found.

---

## 11. Error Handling

**Current status:** Global exception handler maps every known exception
type to a Problem Details response with a stable `type`/`title`/`status`;
the unmapped fallback returns a generic message plus a `traceId` and logs
the real exception server-side only. Already matches the brief's requested
shape closely (uses RFC 7807's `type`/`title`/`detail` rather than a custom
`error.code` envelope — functionally equivalent, and changing the wire
format now would be a breaking change to every existing frontend caller for
no real benefit).

No issues found.

---

## 12. Logging

**Current status:** Serilog, structured. Production/non-Development
environments get compact JSON output (`CompactJsonFormatter`), enriched
with `Environment`/`Application` properties (added Phase 12). Every audit
log entry captures actor, action, entity, timestamp — never a secret or
password. `UseSerilogRequestLogging()` uses default options, which never
capture request/response bodies (confirmed no password/token leak risk via
this path).

**Gap:** no per-request correlation ID exposed *to the client* — the
Problem Details `traceId` uses `HttpContext.TraceIdentifier`, which is
real and unique, but there's no `X-Correlation-Id` response header a
frontend/support workflow could reference without parsing an error body.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| No `X-Correlation-Id` response header | LOW | Minor support/debugging friction — the trace ID exists but only surfaces inside error bodies, not on every response | Add the header, reusing the existing `TraceIdentifier` | This pass |

---

## 13. Monitoring / Observability

**Current status:** `/health/live` and `/health/ready` exist (Phase 1),
`/health/ready` checks real Postgres connectivity via `AddNpgSql`. Neither
endpoint leaks configuration/secrets (confirmed: returns plain
"Healthy"/"Unhealthy" text, no detail). No AI-provider health check.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| No AI provider health signal | LOW | If Anthropic is down, users only find out when they try to chat — no advance signal in `/health/ready` | Out of scope to add today without risking false "unhealthy" readiness failures from a third-party outage (readiness probes should reflect *this service's* ability to serve, not a downstream's) — better solved by the moderation/error-message UX already in place (`AiServiceUnavailableException` → a clear message) | Backlog |
| No real APM/error-tracking backend wired | MEDIUM | Structured logs exist but go nowhere but stdout — no Sentry/equivalent, no alerting | Requires choosing and paying for a real service — **user decision** (flagged, not actioned) | User decision |

---

## 14. Testing

**Current status:** 105 backend unit tests + 4 real integration tests
(genuinely running against Postgres — Testcontainers in CI, verified green
on GitHub's real infrastructure; local-Postgres fallback for sandboxes
without Docker) + 32 frontend unit tests. Phase 11's audit specifically
targeted ownership checks, admin actions, and state-machine transitions —
not padding for a coverage number. Re-run in this pass: still 105/105,
4/4 (verified via the already-green CI, not re-run against a live DB in
this sandbox — Docker still unavailable here), 32/32.

**Gap vs. this brief:** no frontend component tests, no true end-to-end
browser automation suite (Playwright) checked into the repo — the project's
E2E strategy has been live browser verification during each build phase,
not a repeatable automated suite. This was an explicit, informed scope
decision made with the user during Phase 11 (documented in
`docs/12-testing-audit-phase11.md`), not an oversight.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| No automated E2E suite for the 6 critical flows this brief lists | MEDIUM | Regressions in multi-step flows (registration→onboarding→dream, AI coach→action creation, etc.) would only be caught by manual testing | Out of scope to build a full Playwright suite in this pass given the size of that undertaking — flagged clearly as the top testing gap rather than silently left off this audit | Backlog (large, deserves its own pass) |

---

## 15. Performance

**Current status:** Phase 10 did a real pass — N+1 fixed, `AsNoTracking()`
swept, one missing index added, pagination safety caps added to every
genuinely unbounded cross-user list, in-memory caching added for the AI
prompt-template hot path. Frontend audited clean (no images needing
optimization since none exist yet, fonts already use `next/font`, minimal
`"use client"` boundaries, no data-fetching waterfalls). Dashboard/admin
fan-out request patterns measured, not guessed (32–52ms warm).

No new issues found — this was thoroughly covered and re-verified clean.

---

## 16. Accessibility

**Current status:** Established design-system primitives (`Button`,
`Card`, `Input`/`Field`) all use visible focus rings
(`focus-visible:outline-2 focus-visible:outline-beacon-500`), semantic HTML,
`aria-label`/`aria-current`/`aria-busy` used consistently across nav and
interactive components (confirmed via the admin nav, mobile tab bar, and
form components reviewed this pass and in prior phases). Color is not the
sole status indicator in the components reviewed (e.g. locked-user badge
pairs a color change with the word "Locked", not color alone).

**Unverified in this pass:** a full WCAG 2.2 AA sweep (contrast ratios
measured with a real tool, full keyboard-only walkthrough of every page,
screen-reader testing) was not re-run end-to-end here — this sandbox has no
axe-core/Lighthouse accessibility tooling installed, and a claim of full AA
compliance without running that tooling would be dishonest.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| No automated accessibility tooling (axe/Lighthouse) wired into CI | MEDIUM | Regressions in contrast/semantics/keyboard nav wouldn't be caught automatically | **UNVERIFIED**: full WCAG 2.2 AA compliance. Recommend adding `@axe-core/playwright` or a Lighthouse CI step once an E2E suite exists (see Testing gap above) — they're naturally paired | Backlog, paired with E2E suite |

---

## 17. Mobile / Responsive

**Current status:** Every phase's live verification included a responsive
check at mobile/tablet/desktop breakpoints (documented per-phase in the
conversation history). Tailwind's mobile-first utility classes used
throughout; `MobileTabBar`/`AdminNav`'s mobile variant confirmed rendering
correctly with no horizontal scroll in Phase 8's live verification.

**Unverified in this pass:** the full breakpoint matrix this brief specifies
(320/375/390/430/768/1024/1280/1440/1920px) was not re-tested pixel-by-pixel
in this specific pass — prior phases tested a representative subset
(mobile/tablet/desktop), not all nine breakpoints.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| Full 9-breakpoint matrix not verified this pass | LOW | **UNVERIFIED** at the exact breakpoints requested, though the representative subset tested per-phase gives reasonable confidence given Tailwind's fluid utility approach | Re-verify the specific breakpoints live as part of this pass's final verification | This pass (verification step) |

---

## 18. SEO

**Current status:** Root layout has `Metadata` (title, description) — this
cascades to every route by Next.js default unless a page overrides it, so
technically every page has *some* metadata today, not none.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| No `robots.txt` | HIGH | No explicit crawl policy at all — search engines get no guidance on what to index vs. skip, meaning private app routes (`/app/*`, `/admin/*`) could get crawled and indexed if ever linked from anywhere | Add `robots.ts` disallowing `/app/`, `/admin/`, `/api/` | This pass |
| No `sitemap.xml` | MEDIUM | Public marketing pages (landing, login, register) have no sitemap for discovery | Add a minimal `sitemap.ts` covering the public routes only | This pass |
| No per-page Open Graph/Twitter metadata | LOW | The landing page shares a link poorly (no rich preview) — not launch-blocking | Add OG/Twitter metadata to the landing page specifically | This pass (landing page only) |
| No `noindex` on private app routes | HIGH | Same root cause as the `robots.txt` gap — private user data (dreams, journal, dashboard) has no explicit "don't index me" signal beyond requiring auth (which stops crawlers anyway, but defense-in-depth matters for accidentally-shared links) | Add `robots: { index: false }` metadata to the `/app` and `/admin` layouts | This pass |

---

## 19. Infrastructure / Docker

**Current status (corrected during this pass — see note below):**
`apps/web/Dockerfile` exists (multi-stage, `node:22-alpine` build + runtime).
`apps/api/Waypoint.Api/Dockerfile` **does exist** (committed back in Phase 1)
— an earlier claim in this document and in `docs/13-production-readiness-phase12.md`
that it was missing was wrong, caused by a `find -maxdepth 2` search in this
pass that was too shallow to reach a file three directories deep, compounded
by Phase 12 carrying the same wrong claim forward without re-checking. It's
a real, functional multi-stage build (SDK build stage → aspnet runtime
stage) — but it had never actually been build-verified (no Docker daemon in
any sandbox this project has run in), had no non-root user, no
`HEALTHCHECK`, and CI never attempted to build it. `docker-compose.yml`
exists and orchestrates postgres+api+web with a real Postgres healthcheck
gating API startup.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| API Dockerfile never build-verified anywhere, not even in CI | **CRITICAL** | A broken Dockerfile would not be caught until someone tries to deploy | **Fixing this pass**: adding a CI job that runs `docker compose build` for real on GitHub's Docker-enabled runners — genuine verification, not written-and-hoped | This pass |
| `docker-compose.yml`'s `api` service doesn't pass through `ANTHROPIC_API_KEY` | HIGH | Waypoint Coach fails on every request in a `docker compose up` environment | Add `ANTHROPIC_API_KEY: ${ANTHROPIC_API_KEY}` to the `api` service's environment | This pass |
| Neither Dockerfile runs as non-root | MEDIUM | Standard container hardening practice; not currently followed | Add a non-root user to both Dockerfiles | This pass |
| Neither Dockerfile has a `HEALTHCHECK` | LOW | `docker-compose.yml`'s own healthcheck covers Postgres only; the app containers have no self-reported health | Add `HEALTHCHECK` instructions using the existing `/health/ready` endpoint (api) and a lightweight check (web) | This pass |

---

## 20. CI/CD

**Current status:** Real, verified-green GitHub Actions CI (Phase 12) —
backend (build/unit/integration tests) and frontend (lint/test/build) jobs,
confirmed passing on GitHub's actual infrastructure after a real multi-round
debugging cycle that found and fixed a genuine `WebApplicationFactory`
configuration-timing bug (documented in the conversation and in
`docs/13-production-readiness-phase12.md`).

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| No dependency/security-scan step in CI | MEDIUM | `dotnet list package --vulnerable` and `npm audit` are both clean today (re-verified this pass) but nothing catches a newly-disclosed vulnerability in an existing dependency automatically | Add both commands as CI steps (fail the build on findings) | This pass |
| No Docker build verification in CI | Was CRITICAL, tied to the Dockerfile gap above | Same risk as the missing Dockerfile — nothing currently proves the containers actually build | Add a CI job that runs `docker compose build` for real, GitHub-hosted-runner-verified proof | This pass |
| No CD (automated deploy) step | N/A for this pass | Deploying to any real target is explicitly in this task's stop-list ("deploying to production") | Not actioned — correctly out of scope without the user's explicit go-ahead and a real hosting target | User decision |

---

## 21. Backups / Disaster Recovery

**Current status:** No backup strategy exists or has ever been tested.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| No backup strategy, documented or tested | **CRITICAL** | A single Postgres failure/mistake would permanently lose all user data — dreams, journals, business plans, everything | Document a real strategy in `docs/DISASTER_RECOVERY.md` — but per this task's own honesty rule, a documented *procedure* is not the same as a *tested, working* backup. Will write the procedure clearly and mark the actual backup execution/restore test as **UNVERIFIED** until it's run against a real deployed database, since none exists yet | This pass (procedure); **UNVERIFIED** until a real environment exists to test against |

---

## 22. Data Protection / Privacy

**Current status:** Private-by-default confirmed throughout: Community
posts default-private unless explicitly shared, Journal entries are
always private (no sharing mechanism exists at all), every ownership check
audited in Phase 9's A01 pass returns `NotFoundException` (not `Forbidden`)
on cross-user access attempts specifically to avoid leaking existence.
`PrivacySettings` entity exists giving users explicit control over
profile visibility.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| No data export feature | MEDIUM | Users can delete their account but can't export their own data first — a real gap for a product holding personal journal/dream content, and increasingly a baseline expectation (GDPR Article 20 "right to data portability" if any EU users are in scope) | Out of scope to build a full export pipeline this pass given the size (would need to touch every module) — flagged clearly, not silently dropped | Backlog, high priority |
| No documented data-retention policy | LOW | No stated policy on how long data is kept after account deletion, audit log retention, etc. | Document in the Privacy Policy (see Legal section) | This pass (documentation) |
| **Account deletion doesn't cascade to other modules' data** — found during this pass's live verification, not previously known | **CRITICAL** | `DeleteAccountCommandHandler` publishes `UserDeletedIntegrationEvent`, but only the Users module subscribes to it (deletes the Profile row). Confirmed via a real live test — registered a user, created a Dream, created a Community post, deleted the account through the real UI, then queried the database directly: the Dream, Journal, Goals, Actions, Experiments, BusinessIdeas, Community posts/comments, Mentorship data, AI conversations, and Notifications all remained in the database, now orphaned (referencing a `UserId` that no longer has a login). Confirmed via `information_schema` that no module has a database-level foreign key back to Identity's user table either (correct, deliberate module-boundary design — see docs/03-domain-model.md), so nothing cascades automatically at the DB level; the *only* mechanism for this is the integration-event handler pattern, and it's only implemented for one of ~9 modules. This directly contradicted this same pass's own Privacy Policy draft, which originally claimed account deletion was fully self-service — **that copy has been corrected to describe actual current behavior** (login/profile deleted immediately; other content requires a manual contact-us request in the meantime) rather than left overclaiming. | Add an `INotificationHandler<UserDeletedIntegrationEvent>` to each remaining module (Dreams, Journal, Goals, Actions, Experiments, BusinessIdeas, Community, Mentorship, AI, Notifications), each deleting or anonymizing that module's own rows for the user — following the exact same one-line pattern already proven in `Waypoint.Users.Application/Registration/DeleteProfileOnUserDeleted.cs`. Deliberately **not rushed into this pass**: this is a genuinely irreversible, multi-module bulk-delete operation, and writing it carelessly at the end of a long session risks a real data-loss bug — it deserves its own focused pass with per-module tests, not a hurried mechanical sweep. The Privacy Policy and delete-account UI copy were fixed immediately regardless, since leaving them overclaiming would have been actively dishonest independent of when the underlying feature gets built. | **Next pass — do not claim account deletion is complete until this is done** |

---

## 23. Email

**Current status:** `IEmailSender` port exists; `LoggingEmailSender` is the
only implementation (dev-mode, logs instead of sending). Covered in depth
under Authentication above (this is the single most launch-blocking gap in
the whole audit).

See Authentication section — not duplicating here.

---

## 24. Notifications

**Current status:** Preferences-only, no delivery. Covered in depth under
Architecture above.

See Architecture section.

---

## 25. Payments

**Not applicable.** No payment processing exists anywhere in this
application, and none is in scope — confirmed via repo-wide search, no
Stripe/payment-provider references found anywhere.

---

## 26. Admin

**Current status:** Real admin panel (Phase 8), gated on the `Admin` policy,
covering user management (list/lock/unlock), dream oversight, moderation
queue (dismiss/remove-content/resolve), mentor verification, AI usage
summary, audit log, system health. Every admin action writes an audit
entry (Phase 9 fixed the one inconsistency that existed). Live-verified
end-to-end in Phase 8 and again in Phase 9's fix verification.

No new issues found beyond the `Moderator`-role gap already tracked under
Authorization.

---

## 27. Audit Logging

**Current status:** Login success/failure, account lock/unlock, mentor
verification changes, moderation actions (dismiss/remove/resolve), dream
creation, action completion, experiment results — all captured with actor,
action, entity, and timestamp, never a secret. Genuinely comprehensive.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| Account deletion not audit-logged | MEDIUM | Already tracked under Authentication — duplicated here per this brief's explicit "Account deletion" audit requirement | See Authentication section | This pass |

---

## 28. Legal / Compliance / Trust

**Current status:** **None of these exist.** No Privacy Policy, no Terms of
Service, no Cookie Policy, no AI Usage Disclosure, no Contact/Support page.
No footer links to any of them. The app makes no false claims today (no
"guaranteed income" language found anywhere in a repo-wide search of UI
copy), but the complete absence of these pages is itself a real gap for
launch.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| No Privacy Policy | **CRITICAL** | Legally required in most jurisdictions for any app collecting personal data (which this app does extensively — journal entries, dreams, business plans) | Add a real page with genuine, accurate content (not invented legal claims — will describe what data is actually collected and how it's actually used, based on this codebase's real behavior) | This pass |
| No Terms of Service | **CRITICAL** | No documented terms of use, liability limitation, or acceptable-use policy | Add a real page | This pass |
| No Cookie Policy | HIGH | The app sets real cookies (session, CSRF) — no disclosure of what they are or why | Add a real page | This pass |
| No AI Usage Disclosure | HIGH | Users interacting with Waypoint Coach have no explicit disclosure that they're talking to an AI, that outputs aren't guaranteed accurate, or how their input is used | Add a real page, and reference it from the Coach UI itself | This pass |
| No Contact/Support page | MEDIUM | No way for a real user to reach a human | Add a real page (contact method to be provided by the user, since I don't have a real support email/address to put there — will use a clearly-marked placeholder for that one specific detail) | This pass |

---

## 29. Analytics

**Current status:** No product analytics of any kind exist today — no
event tracking, no activation/retention metrics.

| Issue | Severity | Risk | Fix | Priority |
|---|---|---|---|---|
| No privacy-conscious product analytics | MEDIUM | No visibility into activation, onboarding completion, feature usage — can't measure whether the product is actually working for users post-launch | Out of scope to wire a real analytics *provider* this pass (that's a "purchasing services"-adjacent decision — most real options are paid or require account setup). What's in scope and will be done: design the event taxonomy and a first-party event-logging port (`IProductAnalyticsSink` in the same shape as `IAuditSink`) so the application emits the right events from day one, ready to plug a real backend in later without touching business logic | This pass (taxonomy + emission), **backend integration is a user decision** |

---

## Summary table — everything found, by severity

*(Original findings from the initial pass, before any fixes — see "Final status" below for what
actually happened to each one.)*

| Severity | Count | Items |
|---|---|---|
| CRITICAL | 4 | No real email delivery; missing API Dockerfile (later corrected — it existed, just never build-verified); no backup strategy (documented+tested); no Privacy Policy/ToS |
| HIGH | 7 | No Notifications module; email verification not enforced + no resend endpoint (2 items); `robots.txt` missing; `noindex` missing on private routes; Docker `ANTHROPIC_API_KEY` not passed through; no Cookie Policy; no AI Usage Disclosure |
| MEDIUM | 15 | Role model gap (Moderator); CORS AllowAny*; no AI retry policy; no per-conversation message cap; no AI spend cap (user action); no APM backend (user decision); no E2E suite; no a11y tooling in CI; no data export; account-deletion not audited; sitemap missing; no dependency-scan CI step; Docker non-root; no analytics event emission; no Contact page |
| LOW | 6 | "Public (coming soon)" mislabel; `.gitignore` breadth; no correlation-ID header; no AI health signal; OG/Twitter metadata; Docker HEALTHCHECK |

**Total at the start of this pass: 32 tracked issues.**

## Final status — what actually happened, verified

Every item marked "This pass" above was genuinely fixed and verified (build + test + in most
cases real live-server verification against a real Postgres instance — see the live-verification
notes threaded through this document and `docs/PRODUCTION_CHECKLIST.md` for the full account).
**Two real issues were found only by that live verification, after the original audit was
written** — exactly the kind of thing a written-but-unexecuted plan can't catch:

1. **No frontend route existed for account deletion at all**, despite the backend command being
   built and tested since Phase 9 — found by actually trying to call it through the running app.
   Fixed for real this pass: a BFF proxy route, a real confirm-with-password UI, live-tested
   end-to-end including confirming the new `AccountDeleted` audit entry lands in the database.
2. **Account deletion doesn't cascade to any other module's data** (Dream, journal, goals,
   community posts, etc. all survive account deletion, orphaned) — found by deleting a real test
   account through the real UI and then querying the database directly. This one is **not**
   fixed this pass — see section 22 above for why (a genuinely
   irreversible, multi-module bulk-delete deserves its own careful pass, not a rushed one at the
   end of a long session) — but the Privacy Policy and delete-account UI copy, which had
   overclaimed this worked, were corrected immediately rather than left dishonest.

See `docs/PRODUCTION_CHECKLIST.md` for the complete, final, honestly-marked checklist.
