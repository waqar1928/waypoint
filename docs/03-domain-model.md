# Waypoint — Domain Model & Bounded Contexts

## Architectural style

**Modular monolith**, not microservices. One deployable ASP.NET Core
application, internally partitioned into modules with enforced boundaries
(each module owns its own tables/migrations and is only reached through its
public interfaces/contracts — no cross-module EF navigation properties).
Modules communicate via in-process MediatR requests/notifications, never
direct repository access across boundaries. This keeps the system simple to
run and test now, while allowing any module to be extracted into its own
service later without a rewrite, if the domain ever justifies it.

## Bounded contexts (modules)

| Module | Owns | Phase introduced |
|---|---|---|
| **Identity** | Users, credentials, roles, sessions | 1 |
| **Users** | Profile, preferences, notification/privacy settings | 1 |
| **Dreams** | Dream, DreamStatement, Purpose, Obstacle | 2 |
| **Goals** | Goal, Mission, Project, Milestone | 3 |
| **Actions** | Action (task), Next Best Action selection | 3 |
| **Experiments** | Experiment, ExperimentResult, hypothesis/learning loop | 4 |
| **Journal** | JournalEntry, Reflection | 2 (private, ships alongside Dreams) |
| **BusinessIdeas** | BusinessIdea, BusinessValidation, Business Builder workspace | 5 |
| **AI** | AIConversation, AIMessage, prompt templates, provider abstraction | 6 |
| **Community** | CommunityPost, Comment, privacy-scoped visibility | 7 |
| **Mentorship** | MentorProfile, HelpRequest, responses | 7 |
| **Notifications** | Notification, delivery preferences | cross-cutting, stub in 1 |
| **Achievements** | Achievement, Momentum aggregation | 3–4 |
| **Administration** | Admin views/actions across modules, RBAC enforcement | 8 |
| **Audit** | AuditLog — cross-cutting, written to by every module | 1 |

## Cross-cutting concerns

- **Audit**: every module publishes domain events consumed by an
  `IAuditSink` writer; audit rows reference `EntityType`, `EntityId`,
  `Action`, `ActorUserId`, `Payload` (redacted), `OccurredAt`.
- **Multi-tenancy readiness**: every entity carries a nullable `TenantId`
  from day one, defaulted to a single system tenant in v1, so a future
  team/org workspace feature doesn't require a schema migration of every
  table.
- **Soft delete**: entities the user can "undo" (Dream, Goal, Action,
  JournalEntry, CommunityPost) carry `DeletedAt`; hard deletion is reserved
  for account-deletion cascades (GDPR-style erasure).

## Core entity relationships (Phase 1 in bold, later phases plain)

```
**User** 1───1 **Profile**
User 1───* Dream
Dream 1───1 DreamStatement
Dream 1───1 Purpose
Dream 1───* Obstacle
Dream 1───* Goal
Goal 1───* Mission
Mission 1───* Project
Project 1───* Action
Dream 1───* Milestone
Dream 1───* Experiment
Experiment 1───* ExperimentResult
Dream 1───0..1 BusinessIdea
BusinessIdea 1───* BusinessValidation
User 1───* JournalEntry
JournalEntry 0..1───1 Reflection
User 1───* Achievement
User 1───* CommunityPost
CommunityPost 1───* Comment
User 0..1───1 MentorProfile
User 1───* HelpRequest
User 1───* AIConversation
AIConversation 1───* AIMessage
* ───* AuditLog (polymorphic reference: EntityType + EntityId)
```

## Entity field notes (Phase 1 entities only — full list in 05-database-design.md)

### User (Identity module)
Backed by ASP.NET Core Identity (`AspNetUsers`) — email, normalized email,
password hash, security stamp, lockout fields, email-confirmed flag. Not
duplicated in the `Users` module; `Profile` links 1:1 by `UserId`.

### Profile (Users module)
`DisplayName`, `Bio`, `AvatarUrl`, `TimeZone`, `Locale`,
`OnboardingCompletedAt` (nullable — drives whether `/app/onboarding` or
`/app/dashboard` is shown post-login), plus notification and privacy
preference value objects.

## Standard entity envelope

Every domain entity (all modules) includes:

```
Id            Guid (PK)
TenantId      Guid? (nullable in v1, FK-ready)
CreatedAt     DateTimeOffset
UpdatedAt     DateTimeOffset
CreatedBy     Guid (FK -> User.Id)
UpdatedBy     Guid (FK -> User.Id)
RowVersion    byte[] (optimistic concurrency token)
DeletedAt     DateTimeOffset?  (where soft-delete applies)
```

## Module communication rules

1. A module may query only its own tables directly.
2. Cross-module reads go through a published, versioned read contract
   (e.g. `IDreamSummaryProvider` exposed by Dreams for Goals to consume) —
   never a shared DbContext across module boundaries.
3. Cross-module side effects (e.g. "completing an Action should update
   Momentum") go through MediatR domain notifications, handled
   asynchronously in-process, so Actions has zero compile-time dependency
   on Achievements.
4. The AI module never calls other modules' DbContexts directly; it reads
   through the same read-contracts every other module uses, so prompt
   construction can't leak unintended data.
