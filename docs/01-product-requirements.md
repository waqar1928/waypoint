# Waypoint — Product Requirements Document

## 1. What this document is

This PRD defines an original product, **Waypoint**, inspired by general,
non-copyrightable personal-development and entrepreneurship *concepts*
(discovering purpose, challenging limiting beliefs, validating ideas, planning
in small steps, learning from failure). It does not reproduce, paraphrase, or
adapt any text, exercises, chapter structure, or illustrations from any
third-party book. All terminology, question sets, scoring models, and UX
flows below are original work product created for this project.

Waypoint is **not affiliated with, endorsed by, or associated with** any
author, book, or personal brand, and must never present itself as such.

## 2. One-liner

**Waypoint — the operating system for turning dreams into action.**

Someone arrives not knowing what they want to do with their life. They leave
knowing what their dream is, why it matters, who it's for, what's in their
way, what to test first, and exactly what to do today.

## 3. Brand identity

| Element | Decision |
|---|---|
| Product name | Waypoint |
| Tagline | "The operating system for turning dreams into action." |
| Landing hero (original copy, deliberately *not* reusing any third-party book title) | "You already have a dream. Let's find it." |
| Landing sub-head | "Turn the thing you've been thinking about into something you can actually do." |
| Primary CTA | "Find My Dream" |
| Secondary CTA | "See How Waypoint Works" |
| AI assistant name | Waypoint Coach |
| Idea discovery feature | Idea Studio |
| Validation feature | Experiment Lab |
| Business workspace | Business Builder |
| Momentum/progress system | Momentum |
| Visual journey model | The Waypoint Arc |

Naming deliberately avoids reusing any book title, chapter name, or
signature phrase. Where the build spec used a generic placeholder (e.g. "Dream
Coach", "Challenge My Idea"), we've kept the concept but given it Waypoint
branding so the product reads as a single coherent original system rather
than a set of borrowed labels.

## 4. Problem statement

Most people who want to change their life or start something new get stuck
at the very first step: they can't articulate what they actually want, they
don't trust the idea enough to act on it, and they have no structured way to
turn a vague feeling ("I want more out of life") into a next physical action
they can take today. Generic goal-tracking apps assume the user already knows
their goal. Waypoint starts one level earlier — at the discovery of the
dream itself — and stays with the user through validation and execution.

## 5. Target users

- **The Unclear Starter** — knows something needs to change, has no defined
  direction. Primary onboarding audience.
- **The Aspiring Founder** — has a business idea (or several) and needs to
  validate before committing money or time.
- **The Stalled Doer** — has a goal already but has lost momentum or clarity
  on next steps.
- **Mentors/Experts** (secondary) — experienced people who opt in to answer
  help requests from the community.
- **Admins/Moderators** (internal) — operate and moderate the platform.

## 6. Product pillars (mapped 1:1 to journey stages)

**Discover → Define → Validate → Plan → Act → Learn → Grow** — "The Waypoint
Arc". Every major feature attaches to one stage of this arc so the user
always knows where they are and what's next.

1. **Discover** — conversational onboarding that surfaces interests, skills,
   values, and candidate "Dream Directions."
2. **Define** — turn a chosen direction into a clear Dream Statement with
   purpose, audience, problem, and personal motivation.
3. **Validate** — Experiment Lab and Business Builder test assumptions
   cheaply before big commitments; obstacle discovery names what's in the way.
4. **Plan** — cascade the dream into 5-year vision → 3-year direction →
   1-year goal → 90-day mission → 30-day goal → 7-day goal → next action.
5. **Act** — a dream-native task system with an always-visible "Next Best
   Action."
6. **Learn** — journal, experiment results, and lessons feed back into the
   plan.
7. **Grow** — Momentum tracking, milestones, timeline, and (opt-in)
   community/mentorship.

## 7. Non-goals (v1 / Phase 1 scope)

Phase 1 (this build) delivers the **foundation only**: authentication, user
profile, design system, and application shell. The following are designed
for in the architecture (bounded contexts, DB schema) but are **not built**
in Phase 1: Dream discovery flows, AI Coach, Experiment Lab, Business
Builder, Community, Mentorship, Admin portal. See
[09-phased-plan.md](09-phased-plan.md) for the full rollout.

Explicit non-goals for the whole product (not just Phase 1):

- No reproduction of any third-party book's text, exercises, or illustrations.
- No claim of affiliation with any author or personal brand.
- No AI-generated "objective" scores presented as fact — all AI outputs are
  framed as decision support, never certainty.
- No manipulative gamification (streak-shaming, fake urgency, dark patterns).
- No microservices in v1 — single modular monolith.

## 8. Success criteria

- A new user can go from sign-up to a written Dream Statement and a first
  physical action in under 15 minutes.
- At every point in the product, the user can answer "what's my next best
  action?" in one glance at the dashboard.
- AI outputs are always labeled as suggestions, never as verdicts.
- WCAG 2.2 AA conformance on all core flows.
- No horizontal scroll from 320px to 1920px.

## 9. Copyright & brand-safety guardrails (binding on all future phases)

1. No verbatim or near-verbatim reproduction of any third-party book's
   chapters, exercises, prompts, or illustrations.
2. No use of any third-party book title or author's name in branding,
   marketing copy, or in-product terminology.
3. All coaching prompts, question sets, and scoring rubrics used by Waypoint
   Coach are original content authored for this product and stored as
   versioned templates (see [08-technical-architecture.md](08-technical-architecture.md) §AI
   architecture) — never sourced from or trained to imitate a specific
   author's voice.
4. Any AI-generated viability/fit score must render with a persistent
   disclaimer: *"This is a decision-support estimate, not a guarantee."*
