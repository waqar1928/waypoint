# Waypoint — Phased Implementation Plan

Each phase ends with: build, tests, lint, migration check, API smoke check,
responsive check, security review, accessibility review, performance
review, and a written report of exactly what passed/failed — no phase is
marked done on a claim alone.

| Phase | Scope | Status |
|---|---|---|
| **1. Foundation** | Solution setup, ASP.NET Core Identity auth, Postgres + EF Core, user profile, design system, Next.js app shell | **This build** |
| 2. Dream Discovery | Onboarding (Stage 1–2), Dream creation, Dream Statement, Purpose, reflection | Designed, not built |
| 3. Goals & Actions | Goals, Missions, Projects, Actions, Milestones, Dashboard | Designed, not built |
| 4. Experiment Lab | Experiments, hypotheses, results, learning loop | Designed, not built |
| 5. Business Builder | Business ideas, validation, customers, value proposition, business model | Designed, not built |
| 6. Waypoint Coach (AI) | `IAiService` abstraction, conversations, Dream analysis, Idea Studio, Challenge My Idea | Designed, not built |
| 7. Community & Mentorship | Posts, comments, privacy controls, help requests, mentor profiles | Designed, not built |
| 8. Admin & Analytics | Users, dreams, moderation, mentor verification, AI usage, system health, audit log | Designed, not built |
| 9. Security hardening | Full OWASP pass, pen-test-style review, rate limit tuning | **First formal pass done** — see `docs/10-security-audit-phase9.md` |
| 10. Performance optimization | Core Web Vitals, query tuning, caching, code splitting | **First formal pass done** — see `docs/11-performance-audit-phase10.md` |
| 11. Testing | Full unit/integration/API/frontend/E2E coverage pass | Ongoing per phase, formal pass later |
| 12. Production deployment | CI/CD, environment hardening, observability rollout | Not started |

## Phase 1 exit criteria (this build)

- [ ] `Waypoint.sln` builds clean (`dotnet build`).
- [ ] `dotnet test` green across Identity/Users/Audit module tests + API
      integration tests.
- [ ] EF Core migrations for Identity + Users + Audit apply cleanly to a
      fresh Postgres instance.
- [ ] `POST /auth/register`, `POST /auth/login`, `GET /me/profile`,
      `PUT /me/profile` verified end-to-end.
- [ ] Next.js app builds (`npm run build`), lints clean, type-checks clean.
- [ ] Landing page + `/login` + `/register` + `/app/dashboard` shell verified
      responsive at 375 / 768 / 1440.
- [ ] No secrets committed; `.env.example` present, `.env` git-ignored.
- [ ] Axe/manual a11y pass on auth forms and shell nav (keyboard-only
      traversal, visible focus, labelled inputs).
