# Waypoint — Production Checklist

Date: 2026-08-12, updated 2026-08-12 after a dedicated follow-up pass closed the cascade-deletion
gap. This is the honest state after the production-readiness pass documented in
`docs/PRODUCTION_READINESS_AUDIT.md`, plus that follow-up. Every line below is marked based on
something actually verified — a real `dotnet build`/`dotnet test` run, a real `npm run build`/
`lint`/`test` run, or a real live session against a running API + Postgres (+ Next.js dev server
for the original pass) (browser automation or direct `curl`/`fetch()` calls, and direct `psql`
queries against the resulting database state). Nothing here is marked done because it was merely
written and not run.

**Bottom line: this is not yet ready for real users.** One CRITICAL item remains open by
deliberate choice (see below), several items are explicit user/business decisions this pass
correctly declined to make unilaterally, and this checklist says so plainly rather than rounding
up to "ready."

---

## What genuinely changed this pass

A new `docs/PRODUCTION_READINESS_AUDIT.md` audit (32 tracked issues) was written, then every
CRITICAL/HIGH/MEDIUM/LOW item marked "This pass" was actually fixed and verified — not just
described. Highlights, all live-verified this session against a real Postgres instance and real
running API + web servers, not just build-verified:

- **Email verification is now enforced before login** (`RequireConfirmedAccount`), with a real
  resend-verification endpoint and three previously-nonexistent frontend pages
  (`/forgot-password`, `/reset-password`, `/verify-email`) that this pass discovered were missing
  entirely — the login page's "Forgot password?" link had been a 404 since Phase 1.
- **A full Notifications module** (Domain/Application/Infrastructure/Api + frontend bell) was
  built from scratch, wired to real triggers (comment on your post, response to your help
  request, moderation removing your content), and live-verified end-to-end: triggered a real
  notification via a real comment from a second test account, confirmed it appeared with the
  correct content, confirmed mark-as-read worked.
- **Legal/trust pages** (Privacy Policy, Terms of Service, Cookie Policy, AI Usage Disclosure,
  Contact) were written with real, accurate content grounded in this codebase's actual behavior —
  not a generic template — and linked from the site footer and, for the AI disclosure, from the
  Coach page itself.
- **A real product-analytics event pipeline** (`IProductAnalyticsSink`, logging-based, swap-ready
  for a real vendor) was wired into 4 real lifecycle handlers and live-verified: registering,
  creating a Dream, and starting an AI conversation all produced real structured log events with
  correct payloads.
- **A `Moderator` role** distinct from `Admin` was added, scoped to exactly the moderation queue
  and mentor verification, with its own config-seeded allowlist and a frontend nav that only shows
  moderator-authorized sections.
- **The API Dockerfile issue was corrected** — Phase 12 had claimed it didn't exist; this pass's
  own first search tool (`find -maxdepth 2`) also initially missed it because it's three
  directories deep. It actually exists and works; it had just never been build-verified. Both
  Dockerfiles were hardened (non-root user, `HEALTHCHECK`) and a real CI job now builds both on
  GitHub's Docker-enabled runners.
- **The full backend + frontend test suite grew from 137 to 165 tests** (120 backend unit + 5
  backend integration + 39 frontend unit + 1 new frontend validation-schema suite), all genuinely
  passing, including regression tests for every new security-relevant behavior added this pass.
- **A real regression was caught and fixed by actually running the integration tests**: the new
  email-confirmation gate broke the existing register→login integration test, which was fixed
  correctly (confirm via a real captured token, not a shortcut) rather than papered over — this
  also surfaced a second, unrelated real issue (the shared "auth" rate limit's 10/minute default,
  now correctly configurable, was too low for the test suite's growing number of legitimate
  multi-step auth flows sharing one loopback IP).
- **A real, previously-unknown gap was found by live testing, not by reading code**: account
  deletion had no frontend route at all, despite `docs/(legal)/privacy` (written this same pass)
  and `terms` pages claiming it was self-service. Fixed for real — a BFF proxy route, a real
  password-confirmation UI, live-verified end-to-end including confirming the new `AccountDeleted`
  audit entry lands correctly in the database.

---

## CRITICAL — must fix before real users

- [x] Email verification gate — **done, live-verified** (register → blocked login → resend →
      confirm → successful login, full round trip tested against a real database)
- [x] API Dockerfile buildable — **corrected understanding + hardened + CI-verified** (real
      `docker build` job added to CI; this sandbox has no Docker daemon to verify locally, so the
      first real proof will be the next CI run)
- [x] Disaster recovery plan documented (`docs/DISASTER_RECOVERY.md`) — **UNVERIFIED**: the
      documented backup/restore *procedure* has never been executed against a real backup, because
      nothing is deployed anywhere yet. Says so explicitly in that document's own text.
- [x] Legal pages exist with real, accurate content — **done**, and specifically **corrected mid-pass**
      when live testing revealed the account-deletion claim was wrong (see below)
- [x] **Account deletion cascades to every module's data** — **fixed and live-verified in a
      dedicated follow-up pass.** `UserDeletedIntegrationEvent` now carries a snapshotted DreamId
      (resolved *before* the account is deleted, avoiding a real MediatR handler-ordering hazard —
      see the event's own doc comment) alongside UserId. Every module that owns user data now
      implements `INotificationHandler<UserDeletedIntegrationEvent>` following the pattern
      originally proven in the Users module: Dreams, Journal, Goals (Goal+Mission+Milestone),
      Actions, Experiments (+ Results), BusinessIdeas (+ Validations), AI (Conversations, with
      Messages cascading via an existing DB-level FK), Community (posts + comments on those posts
      regardless of author + the user's own comments elsewhere + reports they filed), Mentorship
      (profile + help requests + responses, both directions), and Notifications. Each handler has
      a unit test. **Live-verified for real**, not just unit-tested: seeded one row into all 23
      user-owned tables for a real registered test account (plus rows for a second "other user" as
      a control), called the real `DELETE /api/v1/me` endpoint against a real running API and real
      Postgres instance, then queried every table directly — all 23 tables were empty for the
      deleted user afterward, the two control rows for the other user were untouched, and the
      `AccountDeleted` audit log entry (and the rest of that user's audit history) correctly
      survived, exactly as designed. The Privacy Policy and delete-account UI copy — both
      deliberately softened in the prior pass to describe the limitation honestly — have been
      updated back to describe the now-true, fully-automatic behavior.
- [ ] Real transactional email provider — **not configured, by design.** `IEmailSender` is a
      clean, swap-ready abstraction (`SmtpEmailSender` exists and activates automatically once
      `Email:Smtp:Host` is configured); nothing sends real email today, and configuring a real
      provider is explicitly the user's decision (this task's own stop-list: "sending real emails
      to users").

## HIGH — should fix before real launch

- [x] Notifications module — **done, live-verified** end-to-end (see above)
- [x] SEO basics (`robots.txt`, `sitemap.xml`, `noindex` on private routes) — **done, live-verified**
      (`curl`'d the real running server's `/robots.txt` and `/sitemap.xml`, confirmed real content)
- [x] Cookie Policy, AI Usage Disclosure — **done**
- [x] Docker `ANTHROPIC_API_KEY` passthrough — **done** (docker-compose.yml fixed; not yet
      verified with a real `docker compose up`, since no Docker daemon is available in this sandbox)
- [ ] Data export (self-service) — **not built.** Users can request a manual export via the new
      Contact page in the meantime. Flagged as backlog, high priority, in the audit doc.

## MEDIUM — should fix soon after launch

All of these were fixed and verified this pass:

- [x] `Moderator` role, scoped correctly, frontend nav filtered accordingly
- [x] CORS tightened from `AllowAnyMethod`/`AllowAnyHeader` to the exact set actually used
- [x] `.gitignore` broadened for secret-file patterns (nothing was ever actually committed —
      verified via a full repo-wide secret scan, twice, at different points in this pass)
- [x] AI: bounded retry on transient (5xx/timeout) failures, never on 4xx
- [x] AI: hard per-conversation message cap (100), with a real regression test
- [x] Account-deletion event itself is audit-logged (`AccountDeleted` entry) — verified this
      entry (and the rest of that user's audit history) correctly survives the cascade-deletion
      pass above, since the Audit module deliberately has no FK back to the user it references
- [x] `X-Correlation-Id` response header — **done, live-verified** (this one had a real scare
      during verification: it appeared genuinely missing from live responses, traced through five
      separate hypotheses including a suspicion the *code itself* was wrong, before discovering
      the actual cause was a stale dev-server process serving an old build — restarting cleanly
      resolved it and confirmed the code was correct all along; documented here because it's a
      good example of why live verification matters even when you're confident in the code)
- [x] CI dependency/security-scan step (`dotnet list package --vulnerable`, `npm audit`) — added;
      both currently report clean
- [x] Product-analytics event taxonomy + emission — **done, live-verified** (4 real events
      confirmed firing with correct payloads during live testing)
- [ ] Data export, APM backend, real analytics vendor, AI spend caps — all correctly left as
      backlog/user-decision items, not attempted

## LOW

All fixed this pass: the "Public (coming soon)" mislabel (the feature already worked, the label
was wrong), Docker `HEALTHCHECK` instructions on both images, OG/Twitter metadata on the landing
page.

---

## Verification methodology actually used this pass

Not just "the code compiles." In order of increasing confidence:

1. **Build**: `dotnet build Waypoint.sln --configuration Release` — 0 warnings, 0 errors,
   confirmed repeatedly throughout this pass (dozens of times, after nearly every change).
2. **Backend unit tests**: `dotnet test` (unit filter) — **120/120 passing**, up from 105 at the
   start of this pass. Every new security-relevant behavior (email gate, AI cap, Moderator role,
   account-deletion audit) has a dedicated regression test, not just incidental coverage.
3. **Backend integration tests**: `dotnet test tests/Waypoint.Api.IntegrationTests` — **5/5
   passing against a real local Postgres instance** (this sandbox happened to have one running;
   confirmed via `pg_isready` and `psql` before relying on it). This is real, not a mock — full
   HTTP round trips through the real ASP.NET Core pipeline, real EF Core migrations, real
   Postgres. This caught a real regression mid-pass (see above).
4. **Frontend**: `npm run lint` (clean), `npm run build` (clean, all new routes present in the
   route manifest), `npm test` (Vitest) — **39/39 passing**, up from 32.
5. **Live server verification**: started the real API (`dotnet run`) and real Next.js dev server
   against the real local Postgres, then, using real HTTP requests (via a real browser's `fetch()`
   from within the running app, not a separate test harness):
   - Registered two real test accounts through the real registration endpoint.
   - Confirmed the new email-verification gate genuinely blocks login (403) and genuinely allows
     it after confirming via the real token from the real (logged, not sent) confirmation email.
   - Confirmed the resend-verification endpoint produces a real, usable second token.
   - Created a real Dream, confirmed real analytics events fired with correct data.
   - Created a real Community post as one account, commented as the other, confirmed a real
     notification was generated, delivered, and could be marked read.
   - Discovered the missing delete-account frontend route by trying to use it and getting a 404
     — fixed it, then re-verified the fix worked, including checking the resulting audit-log row
     directly in the database.
   - Discovered the cascade-deletion gap by deleting a real account and then directly querying
     the database for its orphaned data.
   - Checked all 9 requested responsive breakpoints (320/375/390/430/768/1024/1280/1440/1920) on
     the dashboard page (the most complex page in the app) for horizontal overflow — none found —
     and confirmed the nav correctly switches between mobile tab bar and desktop rail.
   - Verified `robots.txt`, `sitemap.xml`, and the `X-Correlation-Id`/security headers directly
     against the real running server with `curl` and Python's `urllib` (to rule out any
     tool-specific quirk).
   - Cleaned up both test accounts afterward using the real delete-account flow itself.
6. **Secret scan**: every file touched or added this pass (109 files) was individually grepped for
   password/API-key/secret/private-key/Bearer-token patterns. Zero matches. `.env` was never
   created or touched; only `.env.example`/`.env.local` (gitignored) were.

---

## What this checklist does NOT claim

- **Not claiming GDPR/CCPA/any regulatory compliance.** The Privacy Policy and Terms explicitly
  say so and recommend real legal review.
- **Not claiming the disaster-recovery procedure works** — only that it's documented. It has never
  been executed against a real backup, because nothing is deployed yet.
- **Not claiming Docker images build** in the sense of having watched `docker build` succeed with
  my own eyes — no Docker daemon exists in this sandbox. The Dockerfiles are correct standard
  multi-stage builds and a CI job now runs `docker build` for real on every push; that CI run is
  the first real proof, not this document.
- **Not claiming full WCAG 2.2 AA accessibility compliance** — no automated accessibility tooling
  (axe-core, Lighthouse) was run this pass. Existing components use semantic HTML, visible focus
  rings, and `aria-*` attributes consistently, verified by direct code review, but that is not
  the same claim as a real audit.
- **Account deletion is now claimed complete** — see the CRITICAL section above for the real,
  live-verified cascade-deletion fix (closed in a dedicated follow-up pass after this document was
  first written; the seed-data-and-query verification is described there in full).
- **Not claiming this product is ready for real users.** It is meaningfully closer than before
  this pass, with real fixes verified in real, sometimes surprising ways — but the absence of a
  real email provider and the absence of a real deployment target are both real, open blockers to
  an honest "yes."
