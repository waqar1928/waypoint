# Drevia — Product Specification

**Document purpose:** a complete, accurate description of what Drevia actually is today, based on
direct inspection of the source code and hands-on testing of the running application — not the
original planning documents, which describe a larger vision than what's currently built. Every
claim in this document was checked against actual code or actual tested behavior. Where something
couldn't be verified, it's marked as such rather than assumed.

**How this document was produced:** full review of `docs/01` through `docs/13`, direct inspection
of every module's Domain/Application/Api layers, direct inspection of every frontend page and
component referenced below, and a full hands-on test pass through registration, onboarding, Dream
creation, planning, actions, experiments, business validation, community, mentorship, and account
settings in a local environment. Drevia Coach's live model responses were not tested in this pass
(would require production API usage); its design and code path were fully reviewed instead.

**Current as of:** commit `b625de1` ("strengthen dream to action learning loop"). Several sections
below were updated from the original version of this document (written Aug 15) to reflect real
work completed since — the Journal navigation fix, Privacy/Notification settings screens, Coach's
optional progress-context awareness, Community/Mentorship's optional Dream context, and this
document's own subject: Next Best Action scoring, the Learn loop, and the Dream Overview page.
Updates are called out inline rather than silently absorbed into the original text.

---

## 1. Source of Truth: Documentation vs. Implementation

Three categories are used throughout this document:

- **A. CURRENTLY IMPLEMENTED** — real, working, verified by reading the code and/or testing it.
- **B. PARTIALLY IMPLEMENTED / INCOMPLETE** — backend exists but the user-facing experience is
  missing, broken, or reachable in a way that doesn't match how it's described elsewhere in the
  product (e.g. the navigation sidebar).
- **C. PLANNED / DOCUMENTED BUT NOT YET IMPLEMENTED** — described in `docs/`, no corresponding code
  exists at all.

A full accounting is in Section 5 (Feature Inventory) and Section 15 (Documentation vs. Reality).

---

## 2. Product Identity

**Product Name:** Drevia

**What exactly is Drevia?** Drevia is a guided planning application for people who don't yet know
what they want to do and need help figuring it out, then structure for turning that into a plan
and a next physical action. It is not, at its core, a chatbot, a journal, or a goal tracker — it's
a structured system that starts one step before all of those (figuring out the goal itself) and
carries the same "Dream" through discovery, planning, testing, execution, and reflection.

**What problem does it solve?** Most people who want to change their life or start something new
get stuck before step one: they can't articulate what they actually want, don't trust an idea
enough to act on it, and have no structured way to turn a vague feeling into a next physical
action they can take today.

**Who is it for?** Three groups, verified against the actual onboarding flow and feature set:
1. People with no clear direction yet (the primary onboarding audience — the entire Discover flow
   is built for someone who genuinely doesn't know).
2. People with a business idea who haven't validated it (served by Business Builder).
3. People with an existing goal who've lost momentum or clarity on next steps (served by the
   dashboard's "Next Best Action" and the Experiment Lab).

A secondary audience — mentors who opt in to answer other users' questions — is also supported
(Mentorship module), plus an internal audience of admins/moderators (Admin module, not covered in
depth in this document since it wasn't tested this pass).

**Core user need:** clarity, then structure, then a reason to believe the next step is worth
taking.

**Why would someone use Drevia?** Because most tools in this space assume the hard part (knowing
your goal) is already solved. Drevia treats that as the actual starting problem.

**What outcome should a user get?** Per the product's own stated success criteria
(`docs/01-product-requirements.md`): going from sign-up to a written Dream Statement and a first
physical action in well under an hour, and being able to answer "what's my next best action?" in
one glance at any point afterward. Both of these are true in the current build — verified by
testing the full flow start to finish.

**Emotional problem Drevia is trying to solve:** the anxious, stuck feeling of knowing something
needs to change but having no idea where to even start, plus the guilt/frustration of watching
ideas or side projects stall out half-finished.

**Functional problem Drevia is trying to solve:** the absence of a structured pipeline from vague
intent → clear statement → tested assumption → time-boxed plan → concrete daily action.

Drevia should **not** be described as simply "an AI app," "productivity software," "a goal
tracker," or "a journal app" — it is a structured personal-direction-finding and planning system,
with AI (Drevia Coach) as one component inside it, not the product itself. See Section 7 for a
precise accounting of what is and isn't actually AI-driven.

---

## 3. The Core Drevia Concept

The **actual** entity relationships, verified directly against the Domain layer source code (not
assumed from documentation):

```
User
 └── Dream (one active Dream per user, in practice — no UI exists to manage multiple)
       ├── DreamStatement (1:1 — purpose, who it helps, problem, outcome, motivation, impact)
       ├── Goal (1:many — three horizons only: FiveYear, ThreeYear, OneYear)
       │     └── Mission (1:many — a 90-day-scale title with an optional target date)
       │           └── Action (1:many)
       ├── Action (can ALSO attach directly to a Dream, with no Goal or Mission — "quick capture")
       ├── Milestone (1:many — attaches directly to the Dream, NOT nested under Goal/Mission)
       ├── Experiment (1:many) → ExperimentResult (1:many per Experiment)
       └── BusinessIdea (0 or 1 per Dream) → BusinessValidation (1:many, holds the viability score)

User (separately, not nested under Dream)
 ├── JournalEntry (1:many — optionally references a Dream, but is fundamentally owned by the User)
 ├── AiConversation (1:many) → AiMessage (1:many per conversation)
 ├── CommunityPost (1:many) → Comment (1:many per post)
 ├── MentorProfile (0 or 1 — opting in to mentor)
 └── HelpRequest (1:many) → HelpRequestResponse (1:many per request)
```

**This differs from the original documentation in two important ways:**

1. `docs/03-domain-model.md` places `Milestone` under `Dream` directly in its relationship diagram,
   which the code confirms — but `docs/02-user-journey.md` describes a much deeper planning
   cascade (5-Year Vision → 3-Year Direction → 1-Year Goal → 90-Day Mission → 30-Day Goal → 7-Day
   Goal → Next Action, seven levels). The actual `Goal` entity only supports three horizons
   (`FiveYear`, `ThreeYear`, `OneYear`), and `Mission` sits directly under `Goal` with no 30-day or
   7-day layer in between. The real cascade is four levels: **Goal (any horizon) → Mission →
   Action**, with Milestone as a separate, parallel achievement-marker track off the Dream, not
   part of this chain at all.
2. `docs/03-domain-model.md` itself documents a deliberate scope decision: a `Project` layer
   between Mission and Action was designed but never built, "left out of both the schema and the
   module for now rather than built and left unused."

### Diagram: the real current product relationship

```
                     ┌─────────────┐
                     │    Dream    │  (the single anchor for a user's planning)
                     └──────┬──────┘
        ┌──────────┬────────┼────────┬──────────────┬─────────────┐
        ▼          ▼         ▼        ▼              ▼             ▼
    Statement    Goal    Milestone Experiment   BusinessIdea   Action (direct)
                  │      (parallel      │             │
                  ▼       track,        ▼             ▼
               Mission   not nested  ExperimentResult BusinessValidation
                  │      under Goal)                  (viability score)
                  ▼
                Action
```

Journal, AI Conversations, Community Posts, and Help Requests all belong to the **User**, not the
Dream — some optionally reference a Dream by ID, but (see Section 9) that reference is not
currently used anywhere in the product experience.

---

## 4. The Core User Loop

1. **The user starts with:** nothing but a vague sense that something should change (or, for
   returning users, an existing Dream).
2. **The user defines:** a Dream Statement — one clear sentence plus purpose, audience, problem,
   outcome, motivation, and impact, all editable.
3. **Drevia helps them create:** a time-boxed plan (Goals at three horizons, one active Mission),
   and, for business-shaped Dreams, a structured validation workspace.
4. **The user actually does:** Actions — small, tagged, optionally time-boxed tasks. **Updated
   since the original version of this document:** one is always shown as the "Next Best Action,"
   but this is no longer only a manual flag the user has to set — Drevia now computes one from
   priority/impact/difficulty/due-date if nothing is manually pinned, and re-picks automatically
   once the current one is completed.
5. **The user tracks progress:** via the Dashboard, which is a live read of actual current state
   (Dream, Mission, Action, Experiment count, viability score, Learnings count,
   Coach/Community/Mentorship activity counts) — verified by testing, not a static mockup.
6. **The user reflects:** via the Journal panel embedded directly on the Dashboard (see Section 5
   — nav-linked correctly now), and, **new since the original version of this document,**
   automatically — recording an Experiment result or answering the optional reflection prompt when
   completing an Action both write a Learning into the same Journal without a separate step.
7. **AI helps:** through Drevia Coach, a conversational feature that already knows the user's
   Dream (and Business Idea, in the relevant mode) without the user re-explaining it, and — if the
   user opts in for that conversation — can also see recent Actions, Experiments, and Learnings.
8. **The user decides what to do next:** by checking the Dashboard's Next Best Action card or the
   Dream Overview page's "Next Move" section, both driven by the same computed recommendation,
   which changes based on real state (prompts Dream Discovery if there's no Dream yet, prompts
   plan drafting if there's a Dream but no plan, otherwise shows the scored recommendation).

### THE DREVIA LOOP

1. Discover (answer questions about yourself)
2. Get Dream Directions (drafts built from your own words)
3. Define a Dream Statement (edit until it's really yours)
4. Draft a Plan (Goals + a Mission, editable before saving)
5. Add Actions — Drevia recommends which one to do next itself, with a reason
6. Test assumptions cheaply (Experiment Lab) and/or validate a business idea (Business Builder) —
   recording a result automatically becomes a Learning
7. Reflect (Journal, on the Dashboard) — manually, or automatically via the loop above
8. Talk to Coach when stuck (already knows your Dream; optionally, recent progress and Learnings)
9. Check the Dashboard, or the Dream Overview page, for what's next
10. Repeat from step 5 or 6 as the plan evolves

---

## 5. Complete Feature Inventory

Status legend: ✅ Fully implemented · 🟡 Partially implemented · 🔴 Not implemented · 🔵 Planned

### Landing page
**Status:** ✅
**Purpose:** explain the product and drive registration.
**What the user does:** reads, clicks "Find My Dream" or "See How It Works."
**What Drevia does:** serves a static marketing page.
**Data created:** none.
**Connects to:** Registration.
**User value:** sets expectations before signup.
**Limitations:** none found.

### Registration
**Status:** ✅
**Purpose:** create an account.
**What the user does:** enters name, email, password.
**What Drevia does:** creates the account, sends a confirmation email, shows a "Check your email"
confirmation state (does not sign the user in or redirect to the dashboard, since the account
isn't usable until confirmed).
**Data created:** a User/Identity record.
**Connects to:** Email confirmation, Login.
**User value:** low-friction entry.
**Limitations:** none found in this pass.

### Email confirmation
**Status:** ✅
**Purpose:** verify the user owns the email address before allowing login.
**What the user does:** clicks the link in the confirmation email.
**What Drevia does:** marks the account confirmed; login is blocked until this happens.
**Connects to:** Login.
**Limitations:** none found; tested with a real confirmation link end to end.

### Login / Logout
**Status:** ✅
**Purpose:** authenticate returning users.
**What Drevia does:** cookie-based session auth; routes a user with no completed onboarding into
`/onboarding` instead of the dashboard.
**Data created:** a session.
**Limitations:** none found. Logout/login persistence was directly tested — all data (Dream,
plan, actions, experiments, viability score) was intact and correctly reflected after a fresh
login.

### Forgot password / Password reset
**Status:** ✅ (implemented) — **not exercised live in this specific testing pass**
**Purpose:** account recovery.
**What Drevia does:** always responds the same way regardless of whether the email exists (an
anti-account-enumeration measure, confirmed in the backend code), so the response itself never
reveals whether an account exists.
**Limitations:** none identified by code review; live click-through not repeated in this pass
(was verified in an earlier phase of this engagement).

### Dashboard
**Status:** ✅
**Purpose:** answer "where am I, and what's next?" at a glance.
**What the user does:** views it, clicks through to whatever needs attention.
**What Drevia does:** loads the user's real Dream, plan, milestones, journal entries, actions,
experiments, business validations, learnings, Coach conversation count, community post count, and
open help request count, all in parallel, and renders a card for each.
**What Drevia does — real progress line, new since the original version of this document:** the
"Your progress" card previously showed only the static Discover→Grow stage strip, with no
cumulative signal despite the landing page's own marketing copy promising "progress you can
actually see." It now also shows one line of real, computed counts — e.g. *"1 action completed ·
1 experiment run · 2 things learned"* — using actual `actions.filter(status === "completed").length`,
`experiments.length`, and `learnings.length`, with correct singular/plural grammar, verified
end to end against real data rather than a static mockup. The Dream empty-state card (shown when a
user has no Dream yet) also gained a real call to action ("Add your dream" → `/onboarding`) — it
previously had none.
**Data created:** none (read-only page).
**Connects to:** every other feature — this is the hub.
**User value:** directly matches the product's own stated success criterion of always being able
to see the next action in one glance. Verified true by testing.
**Limitations:** none found. This is one of the strongest-built parts of the product.

### Dreams / Dream Statement
**Status:** ✅
**Purpose:** the single anchor for a user's planning.
**What the user does:** picks a direction from onboarding, then edits a 7-field statement (Dream
Statement, Purpose, Who it helps, Problem being solved, Desired outcome, Personal motivation,
Impact) plus a "this is a business" checkbox.
**What Drevia does:** pre-fills all fields from the chosen direction but never locks them.
**What Drevia does — the Dream page itself, updated since the original version of this document:**
the `/app/dream` route previously rendered nothing but the 7-field edit form directly (the same
`DreamStatementForm` component reused verbatim from onboarding) — effectively a raw CRUD screen
with no connection to anything else happening in the account. It's now a composed "Dream Overview"
(`dream-overview.tsx`): the statement/purpose/who-it-helps/problem/outcome render as read prose by
default, with the Dream's stage (Discover/Define/Validate/Plan/Act/Learn/Grow — previously tracked
on the entity but never rendered anywhere in the UI) shown as a pill, plus four composed sections
pulling from other modules: **Current Focus** (the active Mission, from `getMyPlan()`), **Next
Move** (the computed Next Best Action and its rationale, from `getNextBestAction()`), **Active
Experiment** (whichever Experiment is `running`, falling back to `planned`, from
`getMyExperiments()`), and **recent Learnings** (from the new `getRecentLearnings()`). The original
edit form still exists and is one click away via an "Edit" toggle — nothing about editing capability
was removed, only what's shown by default when the page first loads.
**Data created:** a Dream + DreamStatement record.
**Connects to:** everything (see Section 3) — and, as of the Dream Overview update, visibly so on
the Dream's own page, not just in the underlying data model.
**Limitations:** the UI supports one Dream per user in practice — no interface exists to create,
switch between, or manage multiple Dreams simultaneously.

### Goals
**Status:** ✅ (at reduced depth vs. documentation)
**Purpose:** time-boxed direction at three horizons.
**What Drevia does:** "Draft my plan" generates editable 5-year/3-year/1-year text tied to the
Dream Statement's own wording, saved as real Goal records on save.
**Limitations:** three horizons only (FiveYear/ThreeYear/OneYear) — the documented 30-day/7-day
layers do not exist in the schema or the UI.

### Milestones
**Status:** ✅ — **discoverable from the sidebar now, but still not a standalone page**
**Purpose:** mark achievement moments that don't fit neatly into the Goal → Mission → Action chain
(the panel's own subtitle: "Mark the moments worth remembering").
**What the user does:** types a title, adds it; can mark any milestone achieved later.
**What Drevia does:** stores it directly against the Dream (not nested under a Goal or Mission).
**Data created:** a Milestone record.
**Limitations:** **Updated since the original version of this document.** The sidebar nav now has
a "Milestones" entry (`Trophy` icon) that links to `/app/dashboard#milestones`, the same treatment
given to Journal. It's no longer only reachable by scrolling to the bottom of the Dashboard and
hoping to notice it. It still has no dedicated route of its own — this remains a real, smaller gap
(see "What Should You Build Next" in the Founder Guide).

### Missions
**Status:** ✅
**Purpose:** the current 90-day focus.
**What Drevia does:** created as part of the Plan-drafting flow, under a Goal; surfaced on the
Dashboard as "Current Mission."
**Limitations:** the UI only ever shows/creates one Mission through the guided Plan flow; no
separate "add another Mission" interface was found.

### Actions
**Status:** ✅
**Purpose:** the actual to-do list, and the source of the single "Next Best Action."
**What the user does:** adds a title, description, priority, difficulty, estimated impact, time
estimate, due date; can tie it to a Goal and/or Mission, or leave it attached only to the Dream.
**What Drevia does — Next Best Action, updated since the original version of this document:**
previously, any action could be manually flagged `IsNextBestAction` and the Dashboard read that
flag directly — nothing else. That manual pin still exists and still wins if set (`GetNextBestAction/
GetNextBestActionQuery.cs`), but now, if nothing is pinned (or the pinned action gets completed or
cancelled, which clears the flag), `NextBestActionSelector` computes a recommendation instead of
returning nothing:
- **Scoring:** priority (Low/Medium/High) and expected impact (Low/Medium/High) each carry the
  heaviest, equal weight; difficulty is a much lighter tiebreaker favoring easier tasks, so a
  trivial task never outranks an important one; a due date adds urgency — more if overdue, less if
  due within 7 days, none otherwise.
- **Rationale:** the API returns a plain-language `Rationale` string alongside the pick — e.g.
  *"This is next because it's high priority and it's likely to move things forward the most,"* or,
  for a manual pin, *"You marked this as your next move."* This is deterministic template logic,
  not an AI call (see Section 7).
- **Auto-reselection:** completing or cancelling the current pin clears it; the next `GET` for
  "next best action" recomputes fresh from whatever's still open, rather than returning nothing
  until a user manually repicks.
- **Where it's shown:** the Actions list (with the rationale under the "Next best" badge), the
  Dashboard's "Next best action" card, and the new Dream Overview page's "Next move" section.
**What the user also does now:** completing an action offers an optional, always-skippable
"what happened / what did you learn" prompt — see the Journal entry below for what happens if
answered.
**Data created:** an Action (`ActionItem` internally) record.
**Limitations:** none found; this is core, working, and directly verified end to end — created two
actions with different priority/impact, confirmed the higher one was recommended with a correct
rationale, completed it, and confirmed the recommendation correctly re-picked the remaining one.

### Journal
**Status:** ✅ **Updated since the original version of this document — the navigation
inconsistency below is resolved, and Journal has grown a real new capability.**
**Purpose:** private reflection (the product's own copy: "A private space for reflection. Only you
can see this.").
**What the user does:** picks an entry type (Daily, Weekly, Lesson, Win, Failure, Idea, Gratitude,
Vision — all 8 documented types are present) and writes an entry.
**What Drevia does — manual entries:** saves it, lists recent entries, never shares it with any
other feature by default (Coach still cannot read arbitrary Journal entries — see Section 6 and
Section 8 for the one narrow, opt-in exception that now exists).
**What Drevia does — automatic entries, new capability:** `LearningCapturedIntegrationEvent`
(`Waypoint.Common/IntegrationEvents.cs`) is published, and a `Lesson`-type Journal entry is
created automatically, in two cases:
1. **Every time an Experiment result is recorded** — the required `Learning` field on
   `RecordExperimentResultCommand` always triggers this. Not optional; there's no opt-out, since
   recording a result already means the user chose to write that learning.
2. **Optionally, when completing an Action** — a new `AddActionReflectionCommand` lets the user
   answer "what happened" and/or "what did you learn" after marking an action complete. Both
   fields are optional and the whole prompt is always skippable; nothing is recorded if skipped.
`GetRecentLearningsQuery` (`GET /api/v1/journal/learnings`) returns just the `Lesson`-type entries,
capped at 10 — this backs the Learnings list on the new Dream Overview page and the Dashboard's
progress count. This reuses the Journal table and its existing (previously unused) `DreamId`
column and `Lesson` entry type rather than adding a new table.
**Data created:** a JournalEntry, optionally linked to the current Dream (now actually populated by
the automatic paths above, where it was previously always left unset in practice).
**Connects to:** Actions (via the optional reflection prompt) and Experiments (automatically, via
every recorded result) write into Journal through the integration event, not a direct reference
between modules — Actions and Experiments have no project reference to Journal. Drevia Coach can
now optionally read a few recent `Lesson` entries — see Section 6 — but never the rest of a user's
Journal.
**Limitations:** the sidebar navigation previously showed "Journal" with a "Soon" badge while a
fully working panel already existed on the Dashboard — **this has been fixed.** The nav now links
to `/app/dashboard#journal`. There is still no standalone `/app/journal` route; the panel remains
on the Dashboard.

### Drevia Coach / AI Conversations
**Status:** ✅ (see Section 6 for full detail)
**Limitations:** see Section 6.

### Business Ideas / Business Validation
**Status:** ✅
**Purpose:** flesh out and stress-test the business version of a Dream.
**What the user does:** fills in as many of 14 optional fields as they know (problem, customer,
value proposition, solution, business model, market, competitors, pricing, marketing, sales,
operations, technology, financial assumptions, risks).
**What Drevia does:** on request, generates a 0-100 viability estimate with "what's working / weak
spots / still unknown / try next" sections and a persistent "not a guarantee" disclaimer.
**Data created:** a BusinessIdea record (one per Dream) and a BusinessValidation record per
estimate generated.
**Limitations:** the viability estimate is a deterministic scoring rule, not an AI model call (see
Section 7).

### Experiments / Experiment Results
**Status:** ✅
**Purpose:** test an assumption cheaply.
**What the user does:** logs what they want to try, what they expect, and how they'll know it
worked; later records a real outcome including a required `Learning` field.
**What Drevia does, updated since the original version of this document:** recording a result
still always finalizes the experiment's status to `Completed`, and now also automatically
publishes the recorded `Learning` as a Journal entry (see the Journal section above) — this closes
a gap the code itself used to only describe in a doc comment ("recording a result is what closes
the learning loop back into Journal") without actually implementing it. It also fixes a real bug:
Drevia Coach's opt-in progress-context builder was fetching an experiment's `LatestLearning` and
silently dropping it before sending anything to the model — it's now included (see Section 6).
**Data created:** an Experiment record, and an ExperimentResult when a result is recorded.
**Limitations:** the "record a result" flow was fully exercised this pass (create → record result
→ confirm both the auto-created Journal entry and the Dashboard's updated learning count) — the
prior caveat about this step not being exercised no longer applies. Still no "Decision" concept
(e.g. build a prototype / pivot / abandon) — a result stops at outcome + learning, with no field
capturing what happens next; this remains a real, documented gap (see the Founder Guide's
prioritized list).

### Community Posts / Comments
**Status:** ✅
**Purpose:** an opt-in space to share progress.
**What the user does:** posts text with a visibility level (Private, Community, Public), comments
on others' posts, and — **new since the original version of this document** — can optionally check
"Attach my Dream so people can see what this is about" when posting.
**What Drevia does:** `CreatePostCommand` takes a plain `AttachDream` boolean, off by default, not
a client-supplied Dream ID. The handler resolves the signed-in user's own Dream server-side via
`IDreamSummaryProvider.GetForUserAsync` — this was a deliberate security decision, not just a
convenience: accepting a raw `DreamId` from the client would have let a malicious request attach
(and thereby publicly expose) any user's Dream, not just their own. When attached, viewers of the
post see the Dream's title and statement only (a lean `AttachedDreamDto`), not the full Dream
record.
**Data created:** a CommunityPost, optionally a Comment.
**Limitations:** "Public" visibility currently behaves identically to "Community" — there is no
actual external/unauthenticated sharing surface yet (documented honestly in the code itself, which
notes the original 4-tier "followers" concept was never designed since no social graph/follow
system exists).

### Content Reports (Community & Mentorship)
**Status:** ✅
**Purpose:** let users flag inappropriate content.
**What the user does:** picks a reason (e.g. Spam), optionally adds detail, submits.
**What Drevia does:** stores the report for admin review; confirmed with a real inline
confirmation message on submission.
**Connects to:** the Admin moderation queue (not tested this pass — requires an admin account).

### Mentorship (Become a Mentor / Mentor Directory)
**Status:** ✅
**Purpose:** let experienced users opt in to help others.
**What the user does:** lists areas of expertise, years of experience, availability.
**What Drevia does:** immediately lists them in the mentor directory — **no verification step is
required to appear as a mentor.** This is a deliberate trust-signal-not-a-gate design decision,
confirmed both in the code's own comments and in the product's own on-screen copy.
**Limitations:** none found relative to its own stated design; whether "no verification required"
is the right trade-off for a real launch is a product decision, not a bug.

### Help Requests / Responses
**Status:** ✅
**Purpose:** ask a specific question, get help from mentors or the community.
**What the user does:** picks a category, writes a title and body, and — **updated since the
original version of this document, which said this UI always sent `dreamId: null`; that is no
longer accurate** — can optionally check "Attach my Dream so mentors can see what this is about."
Can close their own request later.
**What Drevia does:** same pattern as Community posts above — `CreateHelpRequestCommand` takes an
`AttachDream` boolean, never a client-supplied `DreamId`. The handler resolves the user's own
Dream server-side; a mentor viewing the request sees only the lean title/statement, never the raw
Dream record. See Section 9 for what this does and doesn't change about mentor visibility more
broadly.
**Data created:** a HelpRequest, optionally HelpRequestResponses from others.
**Limitations:** attaching a Dream is opt-in and off by default — most help requests will still
carry no Dream context unless the user actively chooses to include it. This is a deliberate
design choice (see Section 9), not a remaining gap.

### Notifications
**Status:** ✅ (delivery mechanism) / 🟡 (trigger coverage not independently re-verified live this
pass)
**Purpose:** tell a user when something involving them happens.
**What Drevia does:** a bell-icon dropdown; confirmed the correct empty state ("No notifications
yet"). Trigger points that exist in the code: someone comments on your post, someone responds to
your help request, your content is removed by a moderator.
**Limitations:** triggering one live requires a second account interacting with the first; not
independently exercised in this specific pass (was code-verified by locating each trigger site).

### Profile
**Status:** ✅
**Purpose:** manage your name, bio, timezone.
**Limitations:** none found.

### Privacy settings
**Status:** 🟡 **Updated since the original version of this document, which marked this
backend-only with no frontend at all — that is no longer accurate.** A real screen now exists
under Settings.
**What Drevia does:** the backend's `GET`/`PUT /api/v1/me/privacy-settings` endpoints are now
reachable through a real BFF proxy route and a real form (`privacy-settings-form.tsx`), letting a
user set Profile visibility and Dream visibility (Private/Followers/Community/Public).
**What's still missing, disclosed directly in the product's own UI rather than left implicit:**
nothing outside the Users module currently reads `ProfileVisibility` or `DreamVisibility` before
showing that data to someone else — the choice saves correctly but isn't enforced anywhere yet.
"Followers" is also a selectable option with no actual effect, since no follow/social-graph system
exists anywhere in Drevia (the same gap Community's own code documents for its visibility tiers).
**Limitations:** a user can now express a privacy preference but cannot yet rely on Drevia to
actually enforce it.

### Notification preferences
**Status:** 🟡 **Same update as Privacy settings, above — no longer backend-only.** A real screen
(`notification-preferences-form.tsx`) now exists under Settings, wired to the
`GET`/`PUT /api/v1/me/notification-preferences` endpoints via a real proxy route. Same caveat:
choices save correctly, but nothing in the app currently checks them before sending an email — the
UI says so plainly rather than implying enforcement that doesn't exist yet.

### Account deletion
**Status:** ✅
**Purpose:** let a user permanently and completely remove their account.
**What Drevia does:** the confirmation copy explicitly lists everything that gets deleted —
"your login, profile, Dream, journal, goals, actions, experiments, business plans, AI
conversations, community posts and comments, and mentorship activity" — and this was verified
against the actual deletion cascade logic in an earlier phase of this engagement (10 backend
modules participate in the cascade).
**Limitations:** none found; genuinely one of the strongest, most honest parts of the product.

### Idea Studio
**Status:** 🔵 Planned, not implemented at all. No seed content, no UI, no route (`/app/ideas`
returns a 404). The code itself documents this as deferred, with no spec written yet.

### Timeline
**Status:** 🔵 Planned, not implemented at all. No route exists (`/app/timeline` returns a 404).

### Admin panel
**Status:** ✅ implemented (7 pages: Users, Moderation, Dreams, Mentors, AI Usage, System Health,
Audit Log) — **not tested in this pass**, since it requires an admin-role account. Its existence
and page list were confirmed by inspecting the actual route files.

---

## 6. Drevia Coach

**What is Drevia Coach?** A conversational AI feature, built specifically for this product rather
than a generic chat window — it starts every relevant conversation already aware of the user's
actual Dream (and Business Idea, in the relevant mode).

**What problem does it solve?** Gives a user somewhere to think out loud about their Dream or
business idea with something that already has the context, instead of a blank chat box.

**When does it become useful?** Any time a user wants to talk through their situation — general
coaching, a read on their Dream Statement specifically, or a direct stress-test of their business
idea.

**How does a user interact with it?** Three distinct conversation modes, each with its own system
prompt:
1. **Coach** — general conversation. Opens by greeting the user and mentioning their Dream title,
   if they have one.
2. **Dream Analysis** — Coach reflects specifically on the Dream Statement, naming what's vague
   without grading it.
3. **Challenge My Idea** — Coach stress-tests a business idea directly but constructively, never
   predicting success or failure.

**What AI provider is currently used?** Anthropic (Claude), via a dedicated adapter
(`AnthropicAiService`). The architecture supports swapping providers (OpenAI, Azure OpenAI, a
local model) without changing any calling code, but only the Anthropic adapter is actually wired
up and used today.

**What information does it receive?**
- The user's Dream: title, statement, purpose, who it helps, and the named problem.
- In "Challenge My Idea" mode only: the Business Idea's problem, customer, value proposition,
  pricing, and competitors fields.
- **New since the original version of this document — opt-in progress context, general Coach
  topic only:** a checkbox in the Coach UI, *"Let Coach see my recent actions, experiments, and
  learnings, not just my Dream"* — unchecked by default, decided fresh per conversation, never
  persisted as a setting. When checked, `StartConversationCommand.IncludeProgressContext = true`
  causes `BuildProgressContextAsync` to also assemble: a bounded, recent-first summary of Actions
  (title, status, whether it's the current next-best pick) via `IActionsSummaryProvider`; a
  summary of Experiments (idea, status, latest outcome, and now also the latest recorded
  *learning* — previously fetched and silently dropped before reaching the model, now fixed and
  included) via `IExperimentsSummaryProvider`; and a few recent `Lesson`-type Journal entries via
  the new `IJournalSummaryProvider`. This is scoped to the general Coach topic only — Dream
  Analysis and Challenge My Idea ignore the flag even if set, since both already have their own
  single, tight focus.
- The message history of the current conversation only.

**What information does it NOT receive, ever:**
- Journal entries other than the specific `Lesson`-type entries surfaced through the opt-in above
  — and even those only when the user has explicitly checked the box for that conversation. Any
  other entry type (Daily, Weekly, Win, Failure, Idea, Gratitude, Vision) is never sent, opt-in or
  not. This remains a real, code-level boundary, not just a policy statement — confirmed by
  inspecting every consumer of `IJournalRepository` in the codebase.
- Goals or Milestones — still never referenced anywhere in Coach's context-building code.
- Community posts, comments, or Mentorship activity.
- Any other user's data.

**Is its context live or a snapshot?** A **snapshot**, taken once, at the moment a conversation
starts. If the user edits their Dream mid-conversation, Coach has no way to know until a new
conversation is started. The same is true of the opt-in progress context — it's assembled once at
conversation start, not re-fetched as the conversation continues.

**Does it remember previous conversations?** Only within a single conversation (its own message
history). It does not carry memory across separate conversations — starting a new conversation
does not pull in anything said in a previous one, only a fresh Dream/BusinessIdea (and, if opted
in, Actions/Experiments/Learnings) snapshot.

**Does it know Goals / Missions / Actions / Journal / Community / Mentorship activity?** **Updated
since the original version of this document, which answered "no" across the board — that's no
longer fully accurate.** Actions and Experiments: yes, but only for the general Coach topic, and
only when the user checks the opt-in box for that specific conversation. Journal: only the narrow
`Lesson`-type slice, same opt-in gate. Goals, Missions, Community, and Mentorship activity: still
no, confirmed by direct inspection of both command handlers that touch a conversation
(`StartConversationCommandHandler`, `SendMessageCommandHandler`) — none of these appear anywhere
in either one. `SendMessageCommandHandler` in particular builds no context at all beyond the raw
message being sent; it's what keeps the opt-in progress context a one-time snapshot at conversation
start rather than something re-fetched on every message.

**What happens when AI is unavailable / the API key is missing / the request fails?** Three
distinct, deliberately different error paths, all verified in code:
- **API key missing entirely:** a clear message — "Drevia Coach isn't configured yet. The
  ANTHROPIC_API_KEY environment variable is missing." The rest of the app is unaffected.
- **The Anthropic API itself is unreachable (network failure):** retried automatically (up to 3
  attempts, with backoff) before failing with "Drevia Coach couldn't reach the AI service right
  now. Please try again."
- **The Anthropic API responds with an error status** (e.g. an invalid key, no credit balance,
  rate limiting): non-5xx errors fail immediately without retrying (retrying an auth failure or a
  bad request wouldn't help); the user sees "Drevia Coach couldn't get a response right now.
  Please try again." The actual status/response body is logged server-side for diagnosis but never
  shown to the user.
- Every conversation is capped at 100 combined messages, to bound worst-case cost on a single
  long-running conversation.

### "Why would I use Drevia Coach instead of opening ChatGPT?"

**Updated since the original version of this document.** Honest answer, based on what's actually
implemented: because Coach starts already knowing your Dream and (in the relevant mode) your
business idea — you don't have to re-explain your situation every time, its tone/behavior is fixed
by design (warm, question-asking, never a verdict, never claims to be a licensed advisor, never
references any specific book or author), and — if you opt in for that conversation — it can now
also speak to your actual recent Actions, Experiments, and Learnings, not just your Dream
Statement. That's a real, working advantage today, and a broader one than the earlier version of
this document described. What it still does **not** offer over ChatGPT: awareness of Goals or
Milestones, continuity across separate conversations, or any memory of past sessions beyond the
current one — opting in gives Coach a snapshot of *now*, not a running history.

### What would need to change for Drevia Coach to become a genuinely differentiated AI coach?

- **CURRENT CAPABILITY:** context-aware at conversation start (Dream + Business Idea, plus
  optional Actions/Experiments/Learnings per conversation), fixed persona with real guardrails,
  graceful failure handling.
- **DONE SINCE THE ORIGINAL VERSION OF THIS DOCUMENT** (previously listed as future opportunities
  below): pulling in a snapshot of recent Actions/Experiments so Coach can reference actual
  progress; optionally reading recent Journal `Lesson` entries with explicit, per-conversation user
  consent, exactly as originally envisioned — "the architecture already isolates Journal from AI,
  so this would be a deliberate, consent-gated addition, not a default" turned out to be an
  accurate prediction of how it was actually built.
- **STILL A FUTURE OPPORTUNITY:** Goals/Milestones awareness (not yet wired into the opt-in
  context); a persistent memory/summary carried across separate conversations, rather than each
  new conversation starting from a fresh snapshot.

---

## 7. AI vs. Non-AI Features

| Feature | Uses AI model? | Current behavior | Explanation |
|---|---|---|---|
| Dream Direction generation (onboarding) | **No** | Deterministic template logic (`HeuristicDreamDirectionGenerator`) reflects the user's own discovery answers back into pre-written sentence templates; falls back to generic "keep exploring" text when answers are mostly skipped | No call to Anthropic or any model anywhere in this code path |
| Plan cascade (Draft my plan) | **No** | Same pattern — template text built from the Dream Statement's own wording (confirmed by the exact phrasing style: "In five years, '[X]' has become [Y]") | No AI call |
| Business viability estimate | **No** | Deterministic scoring rule (`HeuristicViabilityEstimateGenerator`) based on which of the 14 business-canvas fields are filled in | No AI call |
| Next Best Action recommendation (new since the original version of this document) | **No** | Deterministic scoring (`NextBestActionSelector`) over priority, impact, difficulty, and due-date urgency, with a template-generated plain-language rationale | No AI call — same reasoning as the other rows: a "why" a user can hold you to only works if it's the same reason every time for the same inputs |
| Drevia Coach (all 3 modes) | **Yes** | Real calls to Anthropic's Claude API via `AnthropicAiService` | The only feature in the entire product that calls a live AI model |

**Recommendation for accurate marketing language:** don't describe Drevia broadly as
"AI-powered" without qualification — the Dream Direction, Plan, and Viability Estimate features
are honest, well-designed, and reflect the user's own words back at them, but they are template
logic, not model inference. The accurate claim is: **"Drevia Coach is powered by AI. The rest of
Drevia's guidance is built from your own answers using structured logic."** This is not a weaker
claim — arguably it's a stronger trust signal (predictable, explainable, doesn't hallucinate) — but
it should be described accurately rather than implied to be AI everywhere.

---

## 8. Privacy and Trust

**What Drevia currently promises or technically enforces:**

- **Journal privacy is a real, code-level boundary, with one narrow, opt-in exception — updated
  since the original version of this document, which said no other feature reads Journal data at
  all.** Drevia Coach can now optionally see a few recent `Lesson`-type Journal entries, but only
  when the user explicitly checks a box for that specific conversation (off by default, never
  persisted), and never any entry type other than `Lesson`. Every other feature, and Coach itself
  outside that opt-in, still never reads Journal data — confirmed by inspecting every consumer of
  `IJournalRepository` in the codebase.
- **Account deletion is genuinely complete.** Verified (in an earlier phase of this engagement)
  across all 10 participating backend modules — deleting an account removes data from Identity,
  Users, Dreams, Goals, Actions, Experiments, BusinessIdeas, Journal, AI, and Community/Mentorship.
- **User isolation is enforced at the query level** — every module scopes its own data access to
  the authenticated user; cross-module reads only happen through published, versioned contracts
  (e.g. Coach reads Dream data through `IDreamSummaryProvider`, never a raw database query into the
  Dreams module). The same pattern was used for the newer cross-module writes this document
  describes elsewhere (Actions/Experiments publishing `LearningCapturedIntegrationEvent` for
  Journal to consume) — modules still never reference each other's tables directly.
- **Community/Mentorship's optional Dream-attachment feature was built with the same discipline:**
  `CreatePostCommand` and `CreateHelpRequestCommand` both take a plain boolean, never a
  client-supplied `DreamId` — the server always resolves "your own Dream," never trusting the
  client to say whose Dream to attach. This was a deliberate security decision made while building
  the feature, not an afterthought: a raw client-supplied ID would have let a malicious request
  attach (and thereby expose) any user's Dream to a stranger.
- **Authentication is cookie-based**, not token-in-localStorage, which avoids a common XSS-driven
  session-theft pattern.

**Genuine trust differentiators, verified, not invented:**
- Journal is architecturally isolated from AI except for the single, narrow, opt-in exception
  above — this is a real, checkable claim, not just a promise in a privacy policy, and the
  exception itself is opt-in rather than a default.
- The viability estimate and Dream Directions are honest about what's unknown rather than
  presenting confident-sounding but fabricated detail.
- The Next Best Action recommendation and its stated rationale are both deterministic and
  inspectable — the same inputs always produce the same recommendation and the same reason, unlike
  an AI-generated explanation that could vary or fabricate a justification.

**Updated since the original version of this document — no longer true, and should be removed
from any future messaging that still says otherwise:**
- ~~Users cannot currently control their own privacy settings.~~ A real screen now exists (see
  Section 5's "Privacy settings" entry) — though what it controls still isn't enforced anywhere
  yet, which is the current, accurate caveat to state instead.
- ~~Users cannot currently control notification preferences.~~ Same update — a real screen exists,
  same enforcement caveat applies.

---

## 9. Community and Mentorship

**Are they core parts of Drevia, or supporting features?** Based on the actual code: supporting
features, now **partially integrated** with a user's personal Dream/planning journey — **updated
since the original version of this document, which described them as not integrated at all.**
They're still their own modules with their own data, but a user can now choose, per post or per
help request, to attach a lean view of their Dream.

**Can a mentor understand a user's Dream? Updated since the original version of this document,
which answered "No" here — that's no longer accurate.** `HelpRequest.DreamId` is still an optional
field, but it's no longer hardcoded to `null`. The "Ask for help" form now has a checkbox — *"Attach
my Dream so mentors can see what this is about"* — off by default. When checked,
`CreateHelpRequestCommand`'s `AttachDream` flag causes the server to resolve the user's own Dream
(via `IDreamSummaryProvider.GetForUserAsync`, never a client-supplied ID) and attach a lean
`AttachedDreamDto` — title and statement only, not the full Dream record — visible to anyone
viewing that help request. The same pattern exists for Community posts via `CreatePostCommand`.
**This remains genuinely opt-in:** most requests will still carry no Dream context unless the user
actively chooses to include it: a mentor responding to an un-attached request still has no more
context than a stranger on a random forum would.

**Can a mentor see Goals or Missions?** No — there is no code path anywhere that exposes a user's
Goals or Missions to another user, mentor or otherwise. Attaching a Dream exposes only its title
and statement, nothing about the plan built on top of it.

**Can community interactions influence the user's personal journey?** No — posting to Community or
asking/answering in Mentorship has no effect on Dashboard state, Dream stage, Coach context, or
any other part of the personal planning loop. They are informationally adjacent, not integrated,
even now that they can optionally display Dream context to a viewer.

**Current limitation, stated plainly, updated to reflect what's actually still true:** Community
and Mentorship no longer have *zero* awareness of a user's Dream, but the awareness that exists is
narrow (a title and statement, shown only when the user opts in) and one-directional (a mentor can
see a snippet of your Dream if you choose to attach it; nothing about Community/Mentorship
activity flows back into your own Dashboard, Coach context, or plan). This is a smaller,
more accurate gap than the original version of this document described, not a fully closed one.

---

## 10. Current User Journey

```
Landing page
 ↓
Registration
 ↓
"Check your email" (does not sign in yet)
 ↓
Email confirmation (via emailed link)
 ↓
Login → routed into Onboarding (since no Dream/profile completion exists yet)
 ↓
Discover (up to 15 questions, any can be skipped)
 ↓
Dream Directions (3 drafts shown, pick or edit one)
 ↓
Dream Statement ("Make it yours" — 7 editable fields + business-shaped toggle)
 ↓
Dashboard (now populated: Dream card, "Draft your plan" as Next Best Action)
 ↓
Draft Plan (Goals at 3 horizons + one Mission, editable, then saved)
 ↓
Add Actions — Drevia now recommends which one to do next itself (with a plain-language reason),
recomputing automatically as actions are added or completed; a manual pin still overrides it
 ↓
(From here, non-linear — the user can go to any of:)
   Dream Overview → the Dream page itself, now a living summary: statement, Current Focus,
     Next Move, Active Experiment, and recent Learnings, with editing one click away
   Experiment Lab → log a hypothesis, later record a result (which now automatically becomes
     a Learning)
   Business Builder → fill in the canvas, request a viability estimate
   Drevia Coach → talk through the Dream or challenge the business idea, optionally letting it
     see recent Actions/Experiments/Learnings for that conversation
   Journal panel (on the Dashboard) → reflect privately, and see Learnings that arrived
     automatically from Experiments/Actions alongside anything written directly
   Milestones panel (on the Dashboard, now linked from the sidebar) → mark achievement moments
   Community → post progress, optionally attach Dream context, comment, report
   Mentorship → become a mentor, ask for help (optionally with Dream context attached), respond
     to others
 ↓
Logout / Login → all of the above persists correctly
```

Every step above was directly tested in this engagement except the live Coach conversation
exchange itself (design and error handling verified by code, not by sending a real message this
pass) and Admin-panel functionality (requires a role not tested this pass). The Next Best
Action/Learn-loop/Dream Overview additions were verified end to end against the real running
application (registered an account, created a Dream, created and completed Actions with a
reflection, recorded an Experiment result, and confirmed every piece — the computed
recommendation, its rationale, the auto-created Learnings, and the Dream Overview's rendering —
through the actual Next.js + .NET + Postgres stack, not just unit tests).

---

## 11. Real-Life User Example

*"I have this dream: I want to build small software tools and actually sell them, instead of
letting side projects die half-finished."*

**What would I do in Drevia, using the actual current application?**

1. Register, confirm my email, log in — land straight in Onboarding.
2. Answer the Discover questions honestly (what my week looks like, what drains me, who I admire
   and why) — the more I answer, the more personalized my Dream Directions will be; skipping
   questions gets me generic (but still honest) fallback directions instead.
3. Pick a direction, then rewrite the Dream Statement in my own words: what I'm building, who it
   helps, what problem it solves, why it matters to me.
4. Check the box that says this is business-shaped.
5. On the Dashboard, click "Draft your plan" — get an editable 5-year/3-year/1-year/90-day cascade
   built from my own Dream Statement wording, edit it, save it.
6. Add my first Action: "Message 3 people who do repetitive manual work and ask about their
   biggest annoyance." I don't have to manually mark it Next Best Action — with only one open
   action, Drevia already recommends it, and once I add more, it'll recommend whichever one
   actually scores highest on priority/impact/difficulty/due-date, with a stated reason.
7. Go to Business Builder, fill in what I know (problem, customer, value proposition), request a
   viability estimate — Drevia tells me honestly that pricing, market, and competitors are still
   unknown, and gives me two concrete next steps to fill those gaps.
8. Go to Experiment Lab, log a cheap test: "Post in 2 online communities and see who responds."
   When I record the result and what I learned, that learning automatically becomes a Journal
   entry — I don't have to write it twice.
9. Talk to Drevia Coach in "Challenge My Idea" mode — it already knows my problem, customer, and
   value proposition without me re-typing them, and pushes on my riskiest assumption. If I switch
   to a general coaching conversation and check the "let Coach see my recent progress" box, it can
   now also see the Action I completed and what I learned from the Experiment — **updated since
   the original version of this document, which said this wasn't possible.**
10. Write a private Journal entry on the Dashboard about how the first few conversations with
    potential customers went. **Updated since the original version of this document, which said
    Drevia couldn't connect this back into my plan or Coach at all:** if I check the Coach opt-in
    box, a manually-written Journal entry still isn't included (only `Lesson`-type entries created
    through the automatic Experiment/Action paths above are) — so the loop is real but narrower
    than "everything I write feeds back automatically." The Dream Overview page now shows my
    recent Learnings in one place regardless.
11. Post a progress update to Community, optionally checking "Attach my Dream" so people (and, for
    a help request, a mentor) can see the title and statement of what I'm working on. **Updated
    since the original version of this document, which said this wasn't possible at all** — it's
    real now, though still opt-in: if I don't check the box, my situation stays exactly as
    unexplained to a mentor as before.
12. Log out, come back a week later, log in — everything is exactly where I left it.

This is achievable, today, exactly as described — I ran through nearly this entire sequence myself
in this engagement.

---

## 12. What Makes Drevia Different?

**What Drevia combines that these products normally keep separate:**

| Compared to... | What Drevia adds that they don't have |
|---|---|
| ChatGPT / generic AI assistants | A structured Dream/Goal/Action data model the AI is aware of, so you're not re-explaining your situation every session — plus a fixed, non-generic coaching persona with real anti-hallucination framing (never a verdict) |
| Todo apps | Every task traces back to a Dream and a reason, not just a checklist |
| Goal trackers | Starts one step earlier — helping you find the goal, not just track one you already have |
| Habit trackers | Not built around streaks/repetition at all — Drevia is built around a single, evolving Dream, not daily habit compliance |
| Journaling apps | Private by default, with one narrow, opt-in exception (Coach can see recent Learnings if you choose to share them per conversation) — while still being one click from the same Dashboard as everything else |
| Project management software | Explicitly designed for one person's own life direction, not team coordination — no assignees, no team boards |
| Online communities | Community here is opt-in and secondary, not the point of the product |
| Mentorship platforms | Built-in, no separate signup, and posts/help requests can now optionally carry real Dream context (see Section 9) |

**1. Current differentiators:** the connected Dream-centered data model, now visibly so on the
Dream's own Overview page, not just in the underlying schema; Coach's built-in context awareness,
extended since the original version of this document to optionally include recent
Actions/Experiments/Learnings; a genuinely computed Next Best Action with an inspectable rationale,
not a manual bookmark; the honest, uncertainty-admitting viability/direction generation; the real
architectural Journal-AI privacy boundary, with one narrow, opt-in, user-controlled exception.

**2. Potential differentiators, updated since the original version of this document (some of
these are now real, not just potential):** Coach with awareness of Actions/Experiments/Learnings —
**done**, opt-in per conversation; Journal feeding back into planning — **partially done**:
Experiment/Action learnings now flow into Journal automatically, and from there into Coach's
opt-in context, but Journal still doesn't reshape the Plan or Dream itself; Mentorship with real
Dream context — **done**, opt-in; Coach with Goals/Milestones awareness, and persistent memory
across separate conversations — still not built.

**3. Things that are NOT actually differentiated today:** Community, as currently built, still
reads like a small general forum for anyone who doesn't opt into attaching Dream context — its
deeper uniqueness still depends on integration that's now partially, not fully, built.

---

## 13. Product Positioning

**ONE-SENTENCE DESCRIPTION:** Drevia helps you figure out what you actually want to build, then
turns it into a plan you can start on today.

**30-SECOND DESCRIPTION:** Most planning apps assume you already know your goal — Drevia doesn't.
It starts by helping you figure out what you actually want, turns that into a clear plan with a
90-day focus and a next action you can do right now, lets you test business ideas honestly before
committing money, and gives you an AI coach that already knows your situation instead of a blank
chat box.

**2-MINUTE DESCRIPTION:** see the Founder Guide's equivalent section for the full version; the
short form is the 30-second description above, extended with concrete detail from Sections 5-6 of
this document.

**TARGET USER:** someone who knows something needs to change but doesn't yet know what, or someone
with a business idea they haven't tested, or someone with an existing goal who's lost momentum.

**CORE PROBLEM:** the absence of a structured path from vague intent to a concrete next action.

**CORE VALUE PROPOSITION:** one connected system that carries the same Dream through discovery,
definition, validation, planning, and daily action — instead of five disconnected tools.

**PRIMARY DIFFERENTIATOR:** the Dream as a genuine shared anchor across every feature, verified by
code, not just described in marketing copy.

**SECONDARY DIFFERENTIATORS:** Coach's real context-awareness and guardrails; the honest,
uncertainty-admitting tone of the viability/direction generation; the architectural Journal
privacy boundary.

**On "AI-powered":** do not apply this label to the whole product. Apply it specifically to Drevia
Coach. The rest of the guidance system is real, useful, honest — and built from structured logic,
not a model. See Section 7.

---

## 14. Current Product vs. Product Vision

### CURRENT DREVIA
**Updated since the original version of this document — several rows below moved from "Future" to
"Current."** A single-Dream planning tool: Discover → Dream Statement → Goals (3 horizons) → one
Mission → Actions with a genuinely computed Next Best Action, plus Experiments (whose results now
automatically become Learnings), a Business Builder with a heuristic viability score, a
context-aware AI Coach (Dream + Business Idea always; recent Actions/Experiments/Learnings
optionally, opt-in per conversation), a Journal panel on the Dashboard (now correctly linked from
the sidebar, and now also receiving entries automatically), a living Dream Overview page, opt-in
Community and Mentorship (which can now optionally carry real Dream context), user-facing Privacy
and Notification settings screens, and complete account lifecycle management including
full-cascade deletion.

### FUTURE DREVIA (per documentation and architecture, not yet built)
A seven-level plan cascade (down to 30-day and 7-day goals); an Idea Studio; a Timeline view; Coach
with awareness of Goals/Milestones and persistent memory across separate conversations; enforcement
of the Privacy/Notification settings that now exist but aren't yet checked anywhere; a "Decision"
field on Experiment results so Learn→Adjust closes into a concrete next step; a real "here's what I
understood" moment at the end of onboarding; a weekly check-in; proactive dashboard nudges; a
standalone Milestones page.

| Capability | Current | Future |
|---|---|---|
| Plan depth | Goal (3 horizons) → Mission → Action | 7-level cascade to weekly goals |
| Next Best Action | Computed from priority/impact/difficulty/due-date, with a stated rationale; auto-recomputes on completion | Same, likely with richer signals as the plan model deepens |
| Coach context | Dream + Business Idea always; recent Actions/Experiments/Learnings optionally, opt-in per conversation | Goals/Milestones awareness; persistent memory across separate conversations |
| Journal | Discoverable from the sidebar; automatically receives Learnings from Experiments/Actions; feeds Coach's opt-in context | Still no standalone `/app/journal` route; Journal doesn't yet reshape the Plan itself |
| Community integration | Posts can optionally carry a lean Dream summary (title + statement) | Deeper integration — e.g. surfacing Dream context in a mentor's own profile |
| Mentorship integration | Help requests can optionally carry a lean Dream summary | Same as above |
| AI capability | One feature (Coach) actually calls a model | Possibly more AI-assisted steps, clearly labeled as such |
| Progress tracking | Dashboard reflects real state accurately, including a real progress-count line (actions completed / experiments run / things learned) | Likely richer (trends, history) |
| Personalization | Directions/plan reflect the user's own words when given; Coach's opt-in context personalizes further | Deeper personalization if Coach gains fuller context (Goals/Milestones) |
| Privacy control | Real settings screen exists; choices save but aren't enforced anywhere yet | Actual enforcement — e.g. Dream visibility genuinely restricting who can view it |
| Notification control | Real settings screen exists; choices save but aren't enforced anywhere yet | Actual enforcement — e.g. suppressing emails per the user's stated preference |

---

## 15. Documentation vs. Reality

**DOCUMENTATION SAYS:** the plan cascade is 5-Year Vision → 3-Year Direction → 1-Year Goal →
90-Day Mission → 30-Day Goal → 7-Day Goal → Next Action (`docs/02-user-journey.md`).
**ACTUAL IMPLEMENTATION:** Goal (FiveYear/ThreeYear/OneYear) → Mission → Action. Four levels, not
seven.
**IMPACT:** anyone reading `docs/02` as a current spec would expect more granularity than exists.
**RECOMMENDATION:** update `docs/02-user-journey.md` to describe the actual four-level cascade, or
explicitly label the seven-level version as a future direction.

**DOCUMENTATION SAYS (implied by original PRD phasing, `docs/01`):** Phase 1 was foundation-only;
AI Coach, Community, Mentorship, and Admin were explicitly listed as "not built in Phase 1."
**ACTUAL IMPLEMENTATION:** all of these are now built and working.
**IMPACT:** none negative — the product has simply progressed past what the original PRD's Phase 1
section describes. Worth noting only so nobody mistakes the PRD's phase boundaries for current
status.
**RECOMMENDATION:** none required; this is expected drift in a document written before most of the
build happened.

**RESOLVED since the original version of this document.** DOCUMENTATION/UI SAID: the sidebar
navigation marked "Journal" with a "Soon" badge, and the standalone `/app/journal` route returned
a 404, while a fully working panel already existed on the Dashboard. **This was the most
consequential documentation/reality gap in the product at the time** — the navigation was actively
telling users a working feature didn't exist yet. **Current state:** the nav now links to
`/app/dashboard#journal` (the same fix applied to Milestones' own discoverability gap). No
standalone `/app/journal` route exists yet, but the contradiction — nav claiming the feature
doesn't exist — is gone.

**DOCUMENTATION SAYS (`docs/03-domain-model.md`):** Community's visibility model was originally
sketched with 4 tiers including a "followers" tier.
**ACTUAL IMPLEMENTATION:** 3 tiers (Private/Community/Public), and the code itself documents that
"Public" behaves identically to "Community" today since no follow/social-graph system exists.
**IMPACT:** none — this is already honestly documented in the code's own comments.
**RECOMMENDATION:** none required.

---

## 16. Product Completeness

**Rows below marked "Updated" reflect changes verified since the original version of this
document (commit `b625de1` and the several commits immediately before it).**

| Area | Status | Confidence | Main Gap |
|---|---|---|---|
| Authentication | ✅ Complete | High (tested) | None found |
| Onboarding | ✅ Complete | High (tested) | No "here's what I understood" moment before handoff to Dashboard |
| Dreams | ✅ Complete — **Updated:** now a living Dream Overview, not a raw edit form | High (tested end to end) | Single-Dream-only UI |
| Goals | 🟡 Reduced depth vs. docs | High (tested + code) | Only 3 horizons, no 30/7-day layer |
| Milestones | 🟡 **Updated:** now linked from the sidebar; still no standalone page | High (tested + code) | No standalone route |
| Missions | ✅ Complete | High (tested) | Only one Mission surfaced via guided flow |
| Actions | ✅ Complete — **Updated:** Next Best Action is now genuinely computed, not a manual flag | High (tested end to end against real data) | No "Decision"-style follow-through beyond the optional reflection prompt |
| Journal | ✅ **Updated:** nav-linked correctly now, and automatically receives Learnings from Experiments/Actions | High (tested end to end) | No standalone `/app/journal` route |
| AI Coach | ✅ Complete (design); live responses not re-tested this pass. **Updated:** optional per-conversation Actions/Experiments/Learnings context now exists | Medium-High | Still snapshot-only per conversation, no cross-conversation memory; no Goals/Milestones awareness |
| Business tools | ✅ Complete | High (tested) | Viability score is heuristic, not AI |
| Community | ✅ Complete — **Updated:** posts can now optionally carry a lean Dream summary | High (tested) | Dream context is opt-in and lean (title/statement only); not surfaced elsewhere in the social experience |
| Mentorship | ✅ Complete — **Updated:** help requests can now optionally carry a lean Dream summary; the `dreamId: null` hardcode this row used to flag is gone | High (tested) | Same as Community, above |
| Notifications | 🟡 Delivery works, triggers not live-verified | Medium | Live trigger not re-tested this pass; still no proactive/self-initiated notifications |
| Profile | ✅ Complete | High (tested) | None found |
| Privacy / Notification settings | 🟡 **Updated:** real screens now exist for both, wired to the backend endpoints that already existed | High (tested end to end) | Choices save but nothing in the app enforces them yet |
| Account management (deletion) | ✅ Complete | High (verified earlier phase) | None found |
| Mobile UX | Not evaluated | Low | Out of scope this pass |
| Error handling | ✅ Generally strong | Medium-High | Spot-checked, not exhaustively fuzzed |
| Empty states | ✅ Consistently present — **Updated:** the Dream empty-state card gained a real call to action | High (tested across many features) | A few remaining weak empty states (Notifications, Coach's conversation list) still just state absence |
| Email (dev mode) | ✅ Works via log-based fallback | High (tested) | Real SMTP delivery not exercised this pass |
| AI reliability | ✅ Real retry/error handling exists | High (code-verified) | Live failure modes not re-triggered this pass |

---

*See `docs/DREVIA_FOUNDER_GUIDE.md` for the founder-level summary, priorities, and North Star.*
