# Phase 11 — Testing: First Formal Pass

Date: 2026-08-10. Scope: unit, integration, API, and frontend test coverage across
everything built in Phases 1–10. As with Phases 9 and 10, nothing is marked done on a
claim alone — every fix below was actually run and verified, including one real bug
this pass found in code that had never executed before.

## Before/after coverage

| | Before | After |
|---|---|---|
| Backend unit tests | 57 (9 of 11 modules covered) | **105** (all 11 modules covered) |
| Backend integration tests | 4 written, **0 ever run** (Docker unavailable in every environment used across Phases 1–10) | **4/4 passing**, verified against a real running API + Postgres |
| Frontend automated tests | 0 (manual live-browser verification only) | **32** (Vitest, pure logic) |
| **Total** | **57** | **141** |

## Backend unit test coverage gaps

Two modules had zero test coverage: **Audit** and **Journal**. Added
`tests/Waypoint.Audit.Tests` (6 tests covering `GetAuditLogQueryHandler`'s Take-clamping
and DTO mapping — the write side, `AuditSink`, is already indirectly exercised by every
other module's tests that assert `IAuditSink.RecordAsync` was called correctly) and
`tests/Waypoint.Journal.Tests` (9 tests covering `CreateJournalEntryCommand` and
`GetMyJournalEntriesQuery`, including the validator's length rules).

The 9 already-tested modules had only 1–3 test files each — coverage built
incrementally per-phase, not comprehensively. Audited all 74 command/query handlers
across the codebase, cross-referenced against what's actually asserted-on in existing
tests, and found 59 with zero coverage. Rather than padding for a coverage number,
picked the highest-signal 15 and wrote tests for the ones with real branching logic,
ownership checks, or state transitions — skipping trivial one-line pass-through
queries (e.g. `GetAiUsageSummaryQueryHandler`, which is a single delegate call with no
branching). Added:

- **Admin actions** (`LockUserCommand`/`UnlockUserCommand`,
  `UpdateMentorVerificationCommand`, `DismissReportCommand`/`ResolveReportCommand`/
  `RemoveReportedContentCommand`) — every one asserts the audit trail entry fires with
  the correct actor and action name, since these are exactly the handlers Phase 9
  found were inconsistently audited.
- **Ownership-check handlers** (`UpdateGoalCommand`, `UpdateExperimentStatusCommand`,
  `MarkMilestoneAchievedCommand`, `DeleteCommentCommand`) — each has a test proving a
  cross-owner request is rejected with `NotFoundException` (not `Forbidden`, which
  would leak the resource's existence), matching the pattern Phase 9's A01 audit
  confirmed was used consistently but had never been pinned down in a test.
  `MarkMilestoneAchievedCommand` in particular now has a regression test for the exact
  xmin-concurrency bug Phase 10 found and fixed — it asserts the handler goes through
  the new tracked single-row lookup, not the old full-list scan.
- **Multi-branch state transitions** (`UpdateActionStatusCommand`'s conditional
  `CompletedAt`/`IsNextBestAction` clearing depending on the target status,
  `RemoveReportedContentCommand`'s switch over entity type with a `ConflictException`
  default case).
- **Highest blast-radius handler**: `DeleteAccountCommandHandler` — a destructive,
  irreversible operation gated on password re-verification; tested all three outcomes
  (correct password deletes and publishes the integration event, wrong password
  throws and does nothing, a failed underlying delete throws `ConflictException`
  without publishing the event).
- `SelectDreamDirectionCommandHandler` — the "one dream per user" constraint, which
  only had validator-level tests before this pass, not the actual `ConflictException`
  guard in the handler.

## Integration tests: made runnable, then found a real bug

`WaypointApiFactory` required Testcontainers (Docker), which has never been available
in any environment used across Phases 1–10 — meaning these 4 tests were written in
Phase 1 and have sat completely unexercised for the entire project. Fixed this
properly rather than skipping it: the factory now tries Testcontainers first (the more
hermetic, CI-friendly option) and falls back to a scratch database
(`waypoint_integration_test`, drop-and-recreate per run) on the same local Postgres
server the rest of this project's live verification already uses, when no Docker
daemon is available. Confirmed the fallback never touches the `waypoint` dev database.

First run: 3/4 passed, 1 failed. **Not an application bug** — a test bug that had sat
undetected because the test never ran: `Register_then_login_then_read_profile_then_logout_end_to_end`
reused the CSRF token fetched *before* login for the *post-login* logout call. The
app's own antiforgery design (see `AntiforgeryValidationMiddleware`) correctly binds
tokens to auth state and rejects a pre-login token once the session is authenticated —
exactly the behavior the real frontend client already handles correctly via
`invalidateCsrfToken()` + a fresh fetch after login. Fixed the test to do the same
(fetch a fresh token after login before calling logout) rather than loosening the
application's actual security behavior. All 4/4 pass now.

## Frontend: Vitest unit tests for pure logic

Per user decision, scoped to unit tests only — no component or E2E tests this pass;
live browser verification remains the E2E strategy. Installed Vitest (zero
`next build`/lint interference confirmed), added a `test` script, and wrote 32 tests
across two files:

- `validation.ts` — every zod schema backing every form in the app (register, login,
  profile, dream statement, community post, content report, mentor application, help
  request, etc.) had zero test coverage before this pass. Covered the highest-risk
  rules: the register password's length/uppercase/digit regex rules (the most
  bug-prone kind of validation), optional-field handling, enum rejection, and
  boundary-length rejection (off-by-one at the character limits).
- `api-types.ts` — `isProblemDetails`, the type guard the whole app's error-handling
  path depends on to distinguish an RFC 7807 error body from any other response
  shape.

## Verification

- Backend: `dotnet build` clean (0 warnings, 0 errors); full unit suite green
  (105/105); integration suite green (4/4, confirmed running against real Postgres
  via the local fallback, confirmed the scratch test database is left clean and the
  dev database untouched).
- Frontend: `npm test` green (32/32); `npm run lint` clean; `next build` clean
  (including the TypeScript compile step, which caught an unnecessary
  `@ts-expect-error` directive in one of the new test files and was fixed).

## Deferred (by explicit user decision, not an oversight)

- Component tests (React Testing Library) and end-to-end browser automation
  (Playwright) — user chose "unit tests only" for this pass. Live browser
  verification via the session's preview tooling remains the de facto E2E strategy,
  as it has been every phase since Phase 1.
