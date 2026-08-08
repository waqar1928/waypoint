# Waypoint — User Journey

All prompts and copy below are original content written for Waypoint. None
of it is adapted from any third-party book.

## Journey overview: The Waypoint Arc

```
DISCOVER → DEFINE → VALIDATE → PLAN → ACT → LEARN → GROW
```

The arc is circular in practice: Learn feeds back into Define/Validate/Plan
as the user's understanding sharpens. The dashboard always shows which arc
stage the user is currently in.

---

## Stage 1 — Discover: Who Are You?

Delivered as one conversational card at a time (progressive disclosure),
not a form. Each card: one question, large touch target answers or short
text input, a progress indicator ("Step 2 of 9"), and Back/Skip/Save &
Exit controls.

Original question set:

1. "What does a typical week look like for you right now?" (free text)
2. "Which of these feel true today?" (multi-select chips: *restless,
   curious, stretched thin, ready for a change, comfortable but bored,
   energized, uncertain*)
3. "What do you find yourself doing even when nobody's paying you to do it?"
4. "What kind of problems do you notice that other people seem to walk past?"
5. "Whose work do you admire, and what specifically about it?"
6. "If money weren't a factor for the next two years, how would you spend
   your time?"
7. "What's something you used to enjoy that you've drifted away from?"
8. "What kind of work drains you, regardless of the pay or title?"
9. "What experience do you want to be able to talk about in five years?"

Output: a **Discovery Profile** (interests, energizers, drains, admired
work, problem areas, skills/experience tags) used to seed Stage 2.

## Stage 2 — Discover Your Dream: Dream Directions

Original reflective prompts:

- "What would you love to change, even in a small corner of the world?"
- "What problem would you love to be the person who solved it?"
- "What would you spend your time doing if you knew you couldn't fail?"
- "What would make you proud when you look back in five years?"
- "What would you regret never having tried?"
- "What kind of impact do you want your work to have on other people?"

From the Discovery Profile + these answers, Waypoint Coach proposes 3–5
**Dream Directions**, each with:

- Direction statement (1 sentence)
- Why it might fit (tied to specific answers the user gave)
- Skills already present
- Skills likely needed
- Possible opportunities
- Potential challenges
- A first experiment (smallest possible next step)

The user can **select, edit, merge two directions, or discard and
regenerate**. Nothing is auto-committed.

## Stage 3 — Define: Dream Clarity

The chosen direction is expanded into a structured, user-editable **Dream
Statement**:

| Field | Description |
|---|---|
| Dream Statement | One clear sentence of what they're building/becoming |
| Purpose | Why it matters to them personally |
| Who it helps | The specific person/group |
| Problem being solved | What's broken or missing today |
| Desired outcome | What success looks like |
| Personal motivation | Their "why," in their own words |
| Impact | The ripple effect beyond the immediate outcome |

Every field is pre-filled by AI from prior answers but fully editable —
Waypoint never locks a field the user disagrees with.

## Stage 4 — Validate: Obstacle Discovery

The user tags which categories feel like real barriers: **Money, Knowledge,
Skills, Confidence, Time, Network, Technology, Market, Family
responsibilities, Fear of failure, Lack of clarity**.

For each selected obstacle, Waypoint Coach and the user co-author:

- Obstacle (their words)
- Severity (Low / Medium / High, self-rated)
- Why it matters to them
- A possible approach (not a promise of a solution)
- A first action small enough to do this week

## Stage 5 — Validate: Dream Validation (business dreams only)

If the Dream Direction is business-shaped, the user works through an
interactive checklist covering Problem, Customer, Demand, Competition,
Existing alternatives, Value proposition, Pricing possibilities,
Distribution, Startup cost, Required skills, Risks, First experiment.

Output: a **Dream Viability Estimate** (not a "score" presented as fact) —
rendered with a persistent label: *"This is a decision-support estimate
based on what you've told us — not a guarantee of success."*

## Stage 6 — Plan: Dream → Plan Cascade

```
5-Year Vision → 3-Year Direction → 1-Year Goal → 90-Day Mission
→ 30-Day Goal → 7-Day Goal → Next Action
```

Each level is generated as a draft and requires user confirmation/edit
before it's saved — the system never silently commits a plan on the user's
behalf.

## Stage 7 — Act: The Action System

Dream-native tasks (see domain model). The **Next Best Action** is always
visible: on the dashboard, in a persistent header widget, and as the first
thing shown after login.

## Learn & Grow (ongoing)

Journal, Experiment Lab results, and weekly reflections continuously feed
Momentum and may prompt Waypoint Coach to suggest revisiting Define/Validate
if the user's answers have materially changed.

---

## Information Architecture (site map)

```
/                          Landing (marketing)
/how-it-works
/pricing (future)
/login, /register, /verify-email, /forgot-password, /reset-password

/app                       (authenticated shell)
  /app/onboarding/[step]   Stage 1 & 2 flow
  /app/dashboard           Dream Dashboard (home)
  /app/dream                Dream Statement, Purpose, editable fields
  /app/dream/obstacles      Obstacle Discovery
  /app/dream/validation     Dream Validation (business dreams)
  /app/plan                 Vision → Mission → Goal cascade
  /app/actions               Action System / task board
  /app/experiments           Experiment Lab
  /app/business               Business Builder workspace
  /app/coach                  Waypoint Coach conversation
  /app/ideas                  Idea Studio
  /app/journal                 Private journal
  /app/timeline                 Dream Timeline
  /app/community (future)        Opt-in community feed
  /app/mentorship (future)       Help requests / mentors
  /app/settings                 Profile, privacy, notifications, account

/admin                     (staff-only, separate RBAC surface)
  /admin/users
  /admin/dreams
  /admin/moderation
  /admin/mentors
  /admin/ai-usage
  /admin/system-health
  /admin/audit-log
```

Phase 1 builds the shell, `/login`, `/register`, `/app/dashboard`
(placeholder), and `/app/settings/profile` only — the rest of the map exists
here to keep later phases architecturally consistent.
