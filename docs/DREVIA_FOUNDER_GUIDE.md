# Drevia — Founder Guide

*A plain-English guide to what you've actually built, based on reading the real code and testing
the real application — not the original plans. Where something is only a future idea, this
document says so clearly.*

**Current as of:** commit `b625de1` ("strengthen dream to action learning loop"). This document
describes the product at that point in the codebase's history. Several sections below were updated
from the original version of this guide (written Aug 15) to reflect real work completed since —
those updates are called out inline rather than hidden.

---

## What Is Drevia, Really?

Drevia is not a chatbot. It's not a journal app. It's not a goal tracker with an AI feature bolted
on. It's a **guided system for people who don't yet know what they want, and need help finding
out, then actually doing something about it.**

Almost every planning app on the market assumes the hard part is already solved — that you already
know your goal, and just need help tracking it. Drevia starts one step earlier: it helps you find
the goal in the first place, then carries that same idea (your "Dream") through getting clear on
it, testing it cheaply, breaking it into a real plan, and always knowing what to do today.

**The emotional problem:** that stuck, anxious feeling of knowing something needs to change but
having no idea where to start — plus the frustration of watching side projects and half-formed
ideas quietly die.

**The functional problem:** there's no structured path from "I want more out of life" to a
concrete next action you can take right now.

**Who it's for:**
1. Someone with no clear direction yet — this is who the whole onboarding experience is built for.
2. Someone with a business idea they haven't tested.
3. Someone with an existing goal who's lost momentum.

---

## The Core Idea, In One Diagram

```
Your Dream
   ├── A clear Statement (what, why, who it helps, what problem)
   ├── Goals (5-year, 3-year, 1-year)
   │      └── One current Mission (your 90-day focus)
   │             └── Actions (your actual to-do list — one is always "next")
   ├── Milestones (moments worth remembering, tracked separately)
   ├── Experiments (cheap tests of your assumptions)
   └── A Business profile (if it's business-shaped) → a viability score
```

Everything above hangs off one Dream. Journal, AI conversations, community posts, and mentorship
requests belong to *you* rather than the Dream directly, but you'll mostly interact with all of it
from the same Dashboard.

**Important correction from earlier testing:** I initially reported that Journal had no working
screen at all. That was wrong, and I want to be upfront about the mistake rather than bury it. On
closer inspection, Journal **is fully built and working** — you can write and read entries right
on the Dashboard. At the time, the actual problem was different and, honestly, a bit worse: the
sidebar navigation showed "Journal" labeled "Soon," telling every user the feature didn't exist
yet, when it already did. **This has since been fixed** — the nav now links to the Dashboard's
Journal panel, and Journal itself has grown a real new capability: it now automatically receives
entries from Experiment results and Action reflections (see "Journal / Learnings," below).

---

## The Drevia Loop

1. Answer some honest questions about yourself (Discover)
2. Get a few draft "Dream Directions" built from your own words
3. Pick one, rewrite it until it actually sounds like you (Define)
4. Get an editable plan drafted — 5-year vision down to a 90-day focus
5. Add real tasks — Drevia now recommends which one to do next itself, with a plain-language
   reason, rather than requiring you to manually flag one (see "Next Best Action," below)
6. Test the riskiest assumption cheaply before committing (Experiment Lab), or stress-test the
   whole business idea (Business Builder)
7. Reflect — privately in the Journal panel on your Dashboard, and now also automatically:
   recording an Experiment result or reflecting on a completed Action both feed into the same
   Journal as a "Learning," without a separate step
8. Talk to Drevia Coach when you're stuck — it already knows your Dream, and can now optionally
   see your recent progress and learnings too, if you choose to share that for a given
   conversation (see "Drevia Coach, Honestly," below)
9. Check the Dashboard, or the new Dream Overview page, for what's next
10. Repeat

I personally walked through nearly this entire sequence with a real test account this session —
it works, start to finish.

---

## What's Actually Built vs. What's Still a Plan

**Fully working today**, verified by using it myself:
- Registration, email confirmation, login, logout, password reset (recovery flow reviewed, not
  re-clicked this session), and full logout/login data persistence
- Onboarding, Dream Directions, Dream Statement
- A living **Dream Overview page** — no longer just an 8-field edit form. It now shows your Dream
  statement as readable prose, your Current Focus (Mission), your computed Next Move, your Active
  Experiment, and your most recent Learnings, all in one place, with editing one click away.
- Goals, one Mission, Actions — with a **genuinely computed "Next Best Action,"** not just a
  manual flag (see "Next Best Action," below)
- Milestones — the sidebar now links directly to it (previously it was reachable only if you
  happened to scroll to the bottom of the Dashboard); it's still a Dashboard panel, not a fully
  standalone page, but it's no longer hidden
- Experiment Lab — and recording a result now automatically creates a Learning (see below)
- Business Builder, including a real viability score with honest "here's what's still unknown"
  feedback
- Journal — now correctly linked from the sidebar (previously mislabeled "Soon"), and now also
  automatically receives entries from Experiment results and Action reflections
- Drevia Coach (design and safety behavior fully verified; a live conversation exchange wasn't
  re-tested this specific session, by your own choice, to avoid production cost) — can now
  optionally see a snapshot of your recent Actions, Experiments, and Learnings, if you choose to
  share that for a given conversation
- Community posts, comments, content reporting — posts can now optionally carry your Dream
  context (see below)
- Mentorship — becoming a mentor (no verification gate, opt-in trust model), asking for and
  responding to help; help requests can now optionally carry your Dream context too
- Notifications (the display and empty state work; I didn't personally trigger a live notification
  this session, but the trigger code exists and was reviewed)
- **Privacy settings and Notification preferences** — both now have real screens under Settings,
  wired to the backend endpoints that already existed. One honest caveat, disclosed directly in
  the product itself, not hidden: your choices are saved, but nothing in Drevia currently checks
  them before showing your profile/Dream to someone else or sending you an email. Treat these as
  set-for-later preferences, not enforced controls, until that wiring is built.
- Profile editing and full account deletion (genuinely complete — verified it clears every module,
  not just the obvious ones)

**Not built at all yet, just planned:**
- Idea Studio (no content, no spec written)
- Timeline view
- A deeper plan cascade (the docs describe 7 levels down to weekly goals; only 4 levels exist)
- A standalone Milestones page (it's discoverable now, but still lives on the Dashboard, not its
  own route)
- Actual enforcement of Privacy settings / Notification preferences (the settings save, but
  nothing reads them yet — see above)
- A "here's what I understood" summary moment at the end of onboarding, before it hands off to the
  Dashboard
- A weekly check-in / reflection prompt
- Proactive nudges ("you finished all your actions, what's next?") — today the app only responds
  to what you do, it doesn't reach out on its own

---

## Next Best Action, and the Learning Loop

Two of the most important fixes this round of work made were both about closing gaps between what
Drevia *implied* it did and what it actually did.

**Next Best Action used to be a manual bookmark, not a recommendation.** Actions already carried
priority, difficulty, expected impact, and an optional due date — but the "next best action" the
Dashboard showed was just whichever single action a user had clicked "Make this next" on. Nothing
weighed those signals. Now it does: if nothing is manually pinned, Drevia scores every open action
using priority, impact, difficulty (as a lighter tiebreaker, so an easy task never outranks an
important one), and due-date urgency, and shows you the winner along with a plain-language reason
— for example, *"This is next because it's high priority and it's likely to move things forward
the most,"* or *"it's overdue."* A manual pin still always wins if you set one (framed honestly as
*"You marked this as your next move"*), but you're no longer required to pick one yourself for the
feature to mean anything. It also now re-picks automatically the moment your current pick is
completed, instead of going empty.

**Learnings used to be captured but invisible.** Every Experiment result already required a
"what did you learn" field, but that text was trapped inside that one experiment's card, with
nowhere else it ever surfaced — including, embarrassingly, Drevia Coach's own conversation
context, which fetched an experiment's latest learning and then silently dropped it before
sending anything to the model. Both are fixed now:
- Recording an Experiment result **automatically** creates a Journal entry from that learning — no
  extra step required.
- Completing an Action now offers an **optional, always-skippable** "what happened / what did you
  learn" prompt. If you answer it, that becomes a Journal entry too. If you skip it (the default
  path), nothing is recorded — this was a deliberate choice to avoid turning "mark this done" into
  homework.
- The Dream Overview page now shows your most recent Learnings in one place, and the Dashboard
  shows a real count of them alongside actions completed and experiments run.
- Drevia Coach's dropped-learning bug is fixed, and — only when you check the existing "let Coach
  see my recent progress" box for that conversation — it now also sees a few of your most recent
  Learnings, not just Actions and Experiments.

**What's automatic vs. what's optional, stated plainly:** an Experiment's learning becomes a
Journal entry automatically, every time — there's no opt-out, since you already chose to write it
when recording the result. An Action's reflection is entirely optional and skippable. Coach seeing
any of this (Actions, Experiments, or Learnings) is opt-in per conversation, off by default, and
never persisted as a setting — you decide fresh each time you start a new conversation. Coach
still never sees your private Journal entries beyond the Learnings it's been shown this way.

---

## About "AI-Powered"

This matters enough to call out clearly: **most of Drevia's guidance is not AI.**

Your Dream Directions, your Plan cascade, and your Business Viability score are all built by
careful, rule-based logic that reflects your own words back at you — not a call to an AI model.
This is good, honest engineering (it's predictable, explainable, and never makes things up), but
it is not AI.

**The one feature that genuinely calls an AI model (Anthropic's Claude) is Drevia Coach.**

My recommendation: don't market Drevia broadly as "AI-powered" without being specific. Say
something like *"Drevia Coach is powered by AI. The rest of Drevia's guidance comes from your own
answers, structured to make sense of them."* That's not a weaker pitch — a lot of people are
increasingly skeptical of AI that might be making things up, and "this part isn't AI, it's just
careful logic built from your own words" is a legitimate trust signal.

---

## Drevia Coach, Honestly

Coach is real and well-built, with actual safety behavior coded in, not just promised:
- It never claims to be a licensed therapist, career counselor, or financial advisor.
- It's instructed to frame everything as a suggestion, never a verdict.
- It's explicitly told never to reference any specific book, author, or personal brand.
- It has real defenses against being tricked into ignoring its own instructions.

**What it knows about you:** your Dream (title, statement, purpose, who it helps, the problem),
and — only in "Challenge My Idea" mode — your business profile. It gets this once, at the start of
each conversation. If you update your Dream mid-chat, Coach won't know until you start a new one.

**What it can now optionally know, if you choose to share it:** for the general Coach topic only
(not Dream Analysis or Challenge My Idea, which stay tightly focused), there's a checkbox — *"Let
Coach see my recent actions, experiments, and learnings, not just my Dream"* — unchecked by
default, decided fresh every time you start a conversation, never saved as a setting. When
checked, Coach also receives a small, bounded snapshot: a handful of your recent Actions (title,
status, whether it's your next best action), recent Experiments (idea, status, latest outcome and
what you learned from it), and a few of your most recent Learnings. This is new since the earlier
version of this guide — previously Coach could only ever see your Dream.

**What it still never sees, on purpose:** your private Journal entries beyond the specific
Learnings surfaced through the opt-in above, and never at all unless you check that box. This
remains a real, code-level privacy boundary, not just a policy statement — worth saying
confidently in your marketing, because it's actually true and actually checkable. It also still
never sees your Goals or Milestones.

**Why use it instead of just opening ChatGPT?** Because it already knows your situation without
you re-explaining it, its personality/behavior is fixed and reliable by design, and — if you opt
in — it can now speak to your actual recent progress, not just your Dream statement. What it still
doesn't offer over ChatGPT: memory across separate conversations (each new conversation starts
fresh; opting in gives it a snapshot of *now*, not a running history).

---

## What Makes Drevia Genuinely Different

| Instead of... | Drevia adds... |
|---|---|
| ChatGPT | A real data model behind the AI, so you're not re-explaining yourself every time, plus a fixed coaching style with real guardrails |
| A todo app | Every task ties back to a Dream and a reason |
| A goal tracker | Starts a step earlier — helping you find the goal |
| A habit tracker | No streaks, no daily-compliance pressure — it's built around one evolving Dream |
| A journal app | Private by default — the one narrow exception is Learnings you can optionally let Coach see, per conversation, never on by default |
| A community forum | Mentorship built in without a separate signup, and posts/help requests can now optionally carry real Dream context |

**The strongest real differentiator today:** one connected Dream that actually threads through
planning, testing, and AI coaching — and I mean that literally, not as a slogan. I checked the
actual code, and the Dream really is the shared anchor.

**What used to be the weakest area:** Community and Mentorship not knowing anything about a
user's Dream. This has since been addressed — posts and help requests can now optionally carry
real Dream context, so a mentor responding to your help request can see what you're actually
working on, if you choose to share it. The remaining gap is narrower now: this context still isn't
surfaced anywhere *else* in the social experience (e.g. a mentor's own profile, or a richer
"who's working on what" view) — a smaller opportunity than before, not a fully closed one.

---

## Top Product Problems

The original P0-P2 problems identified in this guide have all since been resolved. Keeping them
here, marked resolved, rather than deleting them — it's useful history, and shows the pattern of
"identify the gap, close it" this guide is meant to support.

**P0 — RESOLVED. Was: the navigation told users Journal doesn't exist ("Soon"), when it was fully
working on the Dashboard.** The nav now links directly to the Dashboard's Journal panel.

**P1 — RESOLVED. Was: users couldn't control their own privacy or notification settings.** Both
now have real screens under Settings. One remaining honest caveat (not a new problem, just worth
restating): the settings save correctly but nothing in the app enforces them yet — see "What's
Actually Built," above.

**P2 — RESOLVED. Was: Community and Mentorship had zero awareness of a user's Dream.** Posts and
help requests can now optionally carry Dream context, resolved via a boolean opt-in the server
resolves against the signed-in user's own Dream — not a client-supplied Dream ID, which would have
been a real security gap (a malicious request could otherwise have attached, and exposed, any
user's Dream).

**P3 — Still open. Documentation drift.**
- **Problem:** the written product plan describes a 7-level plan cascade; only 4 levels exist.
  **Why it matters:** low — the current 4-level version works well and is arguably simpler and
  better. This only matters if the old docs get shown to anyone as current.
  **Recommended direction:** update the docs to match reality, or explicitly mark the deeper
  version as future work.

**What's genuinely open now:** the priorities that matter most today are different from the ones
above — see "What Should You Build Next?" below, which has been updated to reflect this.

---

## What Should You Build Next?

The first four items from the original version of this list are now done. Keeping them here,
marked done, so the list stays a useful record rather than silently dropping history:

- ~~Fix the Journal navigation contradiction.~~ **Done.**
- ~~Build Privacy Settings and Notification Preferences screens.~~ **Done** (with the enforcement
  caveat noted throughout this guide).
- ~~Let Coach optionally see recent Actions/Experiments, not just the Dream.~~ **Done** — and
  extended further than originally scoped: Coach can now also optionally see recent Learnings, not
  just Actions/Experiments.
- ~~Let users optionally attach Dream context to a help request.~~ **Done**, and extended to
  Community posts too, not just Mentorship help requests.

Ranked, based on what's genuinely still open:

1. **Build a real standalone Milestones page**, not just a more discoverable Dashboard panel
   (which is now done). Why: it's a real, working feature that's still easy to miss once you're on
   any other page. Effort: low. Impact: medium.

2. **Give onboarding a real "here's what I understood" moment** before it hands off to the
   Dashboard. Why: the 15-question Discover flow is genuinely good, but it currently ends with a
   templated direction card and a silent redirect — no "welcome, here's your Dream" payoff.
   Dependencies: a real UX flow change, not a small tweak. Effort: medium. Impact: high — this is
   the first impression every new user gets.

3. **Add a "Decision" field to Experiment results** (build a prototype / pivot / abandon / test
   further), so the Learn → Adjust loop closes into a concrete next step instead of stopping at
   "here's what I learned." Dependencies: a schema change plus UI. Effort: medium. Impact: medium-high.

4. **Pass over the weakest remaining empty states** (Notifications, Coach's conversation list) so
   every one gives a real reason to act, matching what Actions/Experiments/Community already do.
   Effort: low. Impact: low-medium, but cheap polish.

5. **Add lightweight proactive dashboard nudges** computed from real data at page load — e.g. "you
   finished all your actions, what's next?" — without needing new background-job infrastructure or
   a stored notification. Effort: medium. Impact: medium — today the app never reaches out on its
   own.

6. **Update `docs/02-user-journey.md`** to describe the actual 4-level plan cascade instead of the
   original 7-level one, or explicitly label the deeper version as future work. Why: keeps your own
   documentation honest for future team members. Effort: very low. Impact: low, but cheap.

7. **Decide deliberately on your "AI-powered" language** and make sure marketing, onboarding copy,
   and the product itself agree on where AI actually is (Coach only). Effort: very low (a
   messaging decision, not a build). Impact: medium — protects trust.

8. **Consider a weekly check-in** — a short, optional reflection prompt (what did you do, what did
   you learn, what's next), summarized back to the user. This is genuinely new surface area, not a
   strengthening of something that exists, so it's ranked lower than items above. Effort:
   medium-high. Impact: medium, depends on whether it fits how users actually return to the app.

9. **Test and confirm real Coach conversation quality end-to-end** (a live conversation, refresh
   behavior, cross-session persistence) — this was explicitly not re-tested in this pass. Effort:
   low (mostly just doing it). Impact: high — it's your core AI feature and deserves a real,
   deliberate QA pass, not just a code review.

10. **Consider whether "no verification required to become a mentor" is the right trade-off** for
    a real launch, versus today's deliberate low-friction/trust-builds-over-time design. Not a bug
    — a real product decision worth revisiting once there's real usage data. Effort: N/A (a
    decision, not a build). Impact: depends on how Mentorship is positioned going forward.

---

## The Drevia North Star

Based on everything actually built and tested, not just the original pitch:

**From not knowing what you want, to knowing exactly what to do next.**

This holds up against the real product: the entire onboarding flow exists to solve "I don't know
what I want," the Dashboard's single most important feature is answering "what do I do next," and
every other feature (Goals, Experiments, Business Builder, Coach) exists to support that same
transformation. This is not "become more productive" — it's specifically about the movement from
uncertainty to clarity to action, which is a narrower, more honest, and more defensible claim than
a generic productivity pitch.

---

## If I Were the Founder of Drevia, These Are the 10 Things I Must Understand

1. **Drevia is not "an AI app."** It's a structured planning system with one real AI feature
   (Coach) inside it. Most of the guidance is honest, rule-based logic reflecting your own words —
   say this accurately in your marketing, it's a strength, not a weakness to hide.

2. **The core loop genuinely works, end to end.** I tested it myself: Dream → Plan → Actions →
   Experiments/Business Builder → Dashboard, with real data that survives logging out and back in.
   This is a real, working product, not a prototype.

3. **Journal already works, and your navigation no longer hides it.** This was your single
   cheapest, highest-visibility fix — it's done. Journal also now automatically receives Learnings
   from Experiment results and optional Action reflections.

4. **Privacy and Notification settings now have real screens.** One honest caveat to keep stating
   clearly: they save correctly, but nothing in the app enforces them yet. Don't claim "you control
   who sees your Dream" until that enforcement is actually built.

5. **Your real differentiator is the connected Dream, verified in the actual code** — not a
   slogan. Every feature genuinely reads from and writes to the same Dream. Protect this as you
   add features; don't let anything become disconnected from it.

6. **Coach is well-built and now less narrow than before.** It always knows your Dream; if a user
   opts in for a given conversation, it can also see a snapshot of recent Actions, Experiments,
   and Learnings. It still doesn't remember across separate conversations — each new conversation
   starts fresh. Be precise about this distinction in how you describe it.

7. **Community and Mentorship are less shallow than before, but still not deeply integrated.**
   Posts and help requests can now optionally carry real Dream context. What's still missing:
   nothing *else* in the social experience surfaces that context yet (mentor profiles, a "who's
   working on what" view). A smaller opportunity than it used to be, not a closed one.

8. **Your written planning documents describe a bigger product than what's built** (a 7-level
   plan cascade vs. 4 real levels, for example). That's completely normal for a product in active
   development — just don't let old docs get mistaken for the current spec.

9. **Account deletion is genuinely, completely built** — across every module, including features
   users can't even see yet (Journal). This is a real trust foundation worth stating with
   confidence.

10. **The honest, uncertainty-admitting tone of your viability scores and Dream Directions is a
    real asset.** As you build more (especially more AI), protect this — it's the thing that makes
    Drevia feel trustworthy rather than like every other tool promising certainty it can't back up.

---

## Final Executive Summary

**What have I actually built?** A real, working, connected planning system: users go from not
knowing their direction to a written Dream, a real plan, concrete actions, and an AI coach that
already knows their situation — verified by using it myself, not just reading the code.

**Why should anyone care?** Because it solves the actual first problem (not knowing what you
want) that every other planning tool skips past, and it does so honestly — admitting uncertainty
instead of faking confidence.

**What makes it different?** One Dream that genuinely threads through every feature, an AI coach
with real, checkable guardrails, and a private-by-architecture Journal — not marketing claims, but
things I verified directly in the code.

**What is missing?** The gaps identified in the earlier version of this guide — the Journal
navigation contradiction, missing privacy/notification controls, and Community/Mentorship having
no Dream awareness — have all since been closed. What's genuinely still missing today: a
standalone Milestones page, a real "welcome, here's your Dream" moment at the end of onboarding, a
way to close an Experiment's learning into a concrete next decision, and any form of proactive
outreach from the app (it currently only ever responds to what you do).

**What should you do next?** The highest-leverage fixes from the original version of this guide
are done. The next highest-leverage work is less about closing embarrassing gaps and more about
depth: give onboarding a real payoff moment, and consider what closes the loop from "I learned
something" to "here's what I'm doing differently" — a Decision on Experiments, or a lightweight way
for the app to check in on its own instead of waiting to be opened.
