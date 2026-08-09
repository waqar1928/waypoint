# Phase 10 — Performance Optimization: First Formal Pass

Date: 2026-08-09/10. Scope: EF Core query patterns, database indexes, unbounded list
endpoints, caching, and Core Web Vitals across everything built in Phases 1–9. Each
section states what was reviewed, what was found, and what changed. As with Phase 9,
nothing is marked clean on a claim alone — see "Verification" for how each fix was
confirmed, including two real bugs the audit itself introduced and caught before they
shipped.

## EF Core query patterns

- **N+1 fixed:** `GetModerationQueueQueryHandler` resolved each report's content
  preview with one `GetPostByIdAsync`/`GetCommentByIdAsync` call per report inside a
  loop — a queue of 50 open reports issued up to 51 queries. Added
  `GetPostsByIdsAsync`/`GetCommentsByIdsAsync` batched lookups to `ICommunityRepository`
  and rewrote the handler to resolve all previews in 3 queries total regardless of
  queue size.
- **`AsNoTracking()` swept across ~20 read-only query methods** in 10 repositories
  (Journal, BusinessIdeas, Goals, Experiments, Audit, AI, Actions, Mentorship, Dreams'
  admin listing, Identity's admin user listing) — every method whose result is only
  ever returned as a DTO, never mutated and saved back through the same call, now skips
  EF's change-tracker snapshotting.
- **Real bug caught by this sweep, not by code review:** `AsNoTracking()` on
  `GoalsRepository.GetMilestonesForDreamAsync` broke `MarkMilestoneAchievedCommand`,
  which loaded the *entire* milestone list, found one by ID in memory, mutated it, and
  saved it back — the only handler in the whole codebase that does this. Every module
  here uses xmin-based optimistic concurrency; an `AsNoTracking()`-loaded entity never
  captures the xmin shadow value, so the later `Update()` call sent a stale/default
  token and the save failed every time with `DbUpdateConcurrencyException: 0 rows
  affected`. Caught via live testing (curl against the real API), not unit tests —
  the existing unit tests mock the repository layer entirely, so they never exercised
  real EF Core change tracking. Fixed properly rather than just reverting: added a
  proper `GetMilestoneByIdAsync` single-row lookup (tracked, no `AsNoTracking`) and
  switched the handler to use it — a genuine improvement (no longer loads the whole
  list to find one row) that also restores correct xmin tracking. Grepped the rest of
  the Application layer for the same pattern (`.FirstOrDefault()`/`.Single()`/`.First()`
  or `foreach` over any of the changed list methods) and found zero other instances.

## Database indexes

- Cross-referenced every repository `WHERE`/`OrderBy` against each module's `HasIndex()`
  calls. Coverage was already strong — 25 indexes across 11 modules, including several
  well-chosen composites (`(DreamId, Status)`, `(ConversationId, CreatedAt)`) and one
  clever partial-unique index enforcing "at most one next-best-action per dream" at
  the DB level.
- **Fixed:** the admin audit-log feed (`GetRecentAsync`) does
  `ORDER BY occurred_at DESC LIMIT n` with no `WHERE` clause at all, but the two
  existing indexes on that table are both composites with a different leading column
  (`EntityType`/`ActorUserId`) — neither helps an unfiltered top-N-by-time scan. Added
  a standalone index on `OccurredAt`. As the log grows (every login, every admin
  action writes to it), this is the difference between an index scan and a full-table
  sort on every audit-log page load.

## Unbounded list endpoints

- Distinguished naturally-bounded lists (a user's own dreams/goals/actions — capped by
  realistic personal usage regardless of total user count) from genuinely
  cross-user, unbounded-growth lists: the community feed, the mentorship board, the
  mentor directory, and the two admin oversight lists (all users, all dreams).
- Applied the same safety-cap pattern the audit log already established
  (`Math.Clamp(request.Take, min, max)` in the handler, `ORDER BY ... LIMIT` pushed
  into the SQL query itself, not applied after full materialization):
  community feed (default 100, max 200), mentorship help requests (default 100, max
  200), mentor directory (default 100, max 5000), admin users (default 5000, max
  20000), admin dreams (default 5000, max 20000).
- **Design note applied carefully:** `GetMentorDirectoryQuery` is reused by both the
  public `/api/v1/mentorship/mentors` endpoint and the admin oversight endpoint. Capping
  its *default* would have silently hidden mentors past the 100th from admins doing
  verification review. The admin endpoint now explicitly passes `Take: 5000` rather
  than relying on the public-facing default — caught by tracing every caller before
  shipping the cap, not after.
- Left the mentor directory's in-memory expertise filtering untouched — it was already
  a documented, conscious scale tradeoff from Phase 7 ("fine at this app's scale,
  revisit with a real search index if the mentor pool grows large"); the new cap
  applies *after* filtering, not before, so a narrow search can't get truncated away
  before it has a chance to match.

## Caching

- No caching layer existed anywhere. Deliberately did **not** add broad HTTP-level
  output caching: nearly every endpoint in this app returns per-user or
  session-scoped data (`isMine` flags, admin state, ownership checks), so a
  shared/keyed-wrong cache entry would risk serving one user's data to another — a
  correctness and security risk that isn't justified by this app's current
  pre-launch traffic.
- **Added:** in-memory caching (`IMemoryCache`, 10-minute sliding TTL) for
  `AnthropicAiService`'s prompt-template lookup — genuinely global reference data
  (same active template for a given key regardless of caller), queried on *every*
  single AI turn (both `StartConversation`'s opening turn and every `SendMessage`
  call), seeded once at startup with no runtime edit path. This is the one query in
  the whole app that's both safe to cache and sits on a real hot path. Also noticed
  `IAiRepository.GetActiveTemplateAsync` is dead code — defined, implemented, never
  called anywhere (the actual template lookup was always inline in
  `AnthropicAiService`, which is why the cache went there instead). Left the dead
  method in place rather than removing it — out of scope for a caching-focused pass.

## Frontend Core Web Vitals

Reviewed for LCP/CLS/INP risk and came back genuinely clean — no fixes needed:
- Zero `<img>` tags and zero `next/image` usage anywhere; `avatarUrl` fields exist on
  several DTOs but are never rendered as an actual image anywhere in the UI yet, so
  there's no image-optimization gap to close.
- Fonts already load via `next/font/google` (self-hosted, preloaded, zero layout
  shift) — the correct approach was already in place since Phase 1.
- `"use client"` boundaries are already minimal and correctly scoped: only 2 of 21
  route-level `page.tsx` files are client components, and both are pre-auth forms
  (login/register) with nothing to server-fetch. Every other route follows the
  correct Server Component pattern (fetch on the server, pass initial data as props
  to client leaf components for interactivity).
- Only one component in the entire frontend uses `useEffect` at all
  (`coach-workspace.tsx`), and both usages are legitimate DOM interaction
  (scroll-into-view, auto-starting a conversation after navigation) — not a
  data-fetching waterfall.
- Production bundle: 1.1 MB total across all chunks, largest single chunk 316 KB
  (the shared framework chunk). Nothing bloated per-route.

## Dashboard/admin fan-out request patterns

The dashboard fires ~11 parallel `Promise.all` requests per load (one per module);
the admin overview fires ~5. Considered consolidating into a single aggregation
endpoint, then measured instead of guessing:

- Dashboard (`/app/dashboard`): 260ms cold, 44–52ms warm.
- Admin overview (`/admin`): 138ms cold, 32–34ms warm.

Both are 50–100x under any reasonable Core Web Vitals budget. `Promise.all` already
runs every request concurrently (not a sequential waterfall), and each individual
backend call is now index-covered, `AsNoTracking()`'d, and capped from the fixes
above. Consolidating into a single aggregation endpoint would require either violating
the project's module-boundary rule (modules communicate only via published read
contracts, never direct cross-module queries) or building a new thin aggregation
layer for a problem that measured data says doesn't exist. Left as-is — this is a
legitimate pass, not a gap papered over.

## Verification

- Backend: `dotnet build` clean (0 warnings, 0 errors) after every change; full unit
  test suite green (57/57) after all fixes.
- Frontend: `npm run lint` clean; no frontend source changes this phase (Phase 10 was
  backend-query-focused), confirmed via `next build` that nothing regressed.
- Live-verified against the running API (real Postgres, real session cookies):
  - The milestone concurrency bug: reproduced the `DbUpdateConcurrencyException` via
    curl before the fix, then confirmed `POST
    /api/v1/milestones/{id}/achieve` returns 200 with `achievedAt` set after the
    fix.
  - The moderation N+1 fix: created a post, reported it, confirmed
    `contentPreview` is still correctly populated via the batched lookup (not just
    "doesn't crash" — the actual preview text came through).
  - The admin/public mentor-directory cap split: confirmed both endpoints return the
    same (currently small) mentor list, since the real risk (public cap silently
    hiding mentors from admins) only manifests once the mentor count exceeds 100 —
    verified the *code path* takes the explicit `Take: 5000` argument rather than
    the default by reading the endpoint mapping, since the current data volume can't
    exercise the difference yet.
  - The AI prompt-template cache: started a real conversation (cache miss, populated)
    then sent a follow-up message in the same conversation (cache hit) — both
    produced real Anthropic responses, confirming the cache doesn't break the AI
    request path in either state.
  - Confirmed the new audit-log index migration applied cleanly on API startup
    (`Waypoint:AutoMigrate`) and the audit-log endpoint still returns data correctly
    afterward.
  - Timed the dashboard and admin-overview fan-out patterns directly against the
    live dev server rather than estimating.

## Deferred / not gaps (by design)

- Broad HTTP output caching — deliberately not added; see "Caching" above.
- Dashboard/admin request consolidation — deliberately not added; see "Dashboard
  fan-out" above, backed by real timing data.
- Mentor directory's in-memory expertise filtering — pre-existing, documented Phase 7
  scale tradeoff, unchanged.
- `IAiRepository.GetActiveTemplateAsync` dead code — noted, not removed; out of scope
  for this pass.
