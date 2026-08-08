# Waypoint — Database Design

Target: PostgreSQL 16, accessed via EF Core. Schema shown as DDL-equivalent
for clarity; actual source of truth is EF Core migrations
(`src/Modules/*/Infrastructure/Migrations`).

Conventions used throughout:
- `uuid` primary keys (`gen_random_uuid()`), not identity ints — avoids
  cross-module/tenant collision and leaks no sequential info externally.
- `timestamptz` for all timestamps.
- Every table has `created_at, updated_at, created_by, updated_by,
  row_version, tenant_id`. Tables the user can undo also have `deleted_at`.
- Foreign keys `ON DELETE RESTRICT` by default; explicit `CASCADE` only
  where the child has no independent meaning (e.g. `AIMessage` under
  `AIConversation`).

## Phase 1 schema (implemented now)

```sql
-- Identity module — owned by ASP.NET Core Identity, standard tables:
-- asp_net_users, asp_net_roles, asp_net_user_roles, asp_net_user_claims,
-- asp_net_user_logins, asp_net_user_tokens, asp_net_role_claims
-- (created by Identity's own migrations; not hand-rolled here)

CREATE TABLE users_profile (
    id                      uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id                 uuid NOT NULL UNIQUE REFERENCES asp_net_users(id),
    tenant_id               uuid NULL,
    display_name            varchar(120) NOT NULL,
    bio                     varchar(500) NULL,
    avatar_url              varchar(2048) NULL,
    time_zone               varchar(64) NOT NULL DEFAULT 'UTC',
    locale                  varchar(16) NOT NULL DEFAULT 'en-US',
    onboarding_completed_at timestamptz NULL,
    created_at              timestamptz NOT NULL DEFAULT now(),
    updated_at              timestamptz NOT NULL DEFAULT now(),
    created_by              uuid NOT NULL,
    updated_by              uuid NOT NULL,
    row_version             bytea NOT NULL
);
CREATE INDEX ix_users_profile_user_id ON users_profile(user_id);

CREATE TABLE users_notification_preferences (
    id                      uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id                 uuid NOT NULL UNIQUE REFERENCES asp_net_users(id),
    email_product_updates   boolean NOT NULL DEFAULT true,
    email_coach_nudges      boolean NOT NULL DEFAULT true,
    email_community_activity boolean NOT NULL DEFAULT false,
    created_at              timestamptz NOT NULL DEFAULT now(),
    updated_at              timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE users_privacy_settings (
    id                      uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id                 uuid NOT NULL UNIQUE REFERENCES asp_net_users(id),
    profile_visibility      varchar(20) NOT NULL DEFAULT 'private'
        CHECK (profile_visibility IN ('private','followers','community','public')),
    dream_visibility        varchar(20) NOT NULL DEFAULT 'private'
        CHECK (dream_visibility IN ('private','followers','community','public')),
    created_at              timestamptz NOT NULL DEFAULT now(),
    updated_at              timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE audit_log (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id       uuid NULL,
    entity_type     varchar(100) NOT NULL,
    entity_id       uuid NOT NULL,
    action          varchar(50) NOT NULL,       -- Created/Updated/Deleted/LoginSucceeded/...
    actor_user_id   uuid NULL,
    payload_redacted jsonb NULL,
    occurred_at     timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX ix_audit_log_entity ON audit_log(entity_type, entity_id);
CREATE INDEX ix_audit_log_actor ON audit_log(actor_user_id, occurred_at DESC);
```

## Later-phase schema (designed now, migrated when the phase lands)

```sql
CREATE TABLE dreams (
    id                  uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id           uuid NULL,
    user_id             uuid NOT NULL REFERENCES asp_net_users(id),
    title               varchar(200) NOT NULL,
    stage               varchar(20) NOT NULL DEFAULT 'discover'
        CHECK (stage IN ('discover','define','validate','plan','act','learn','grow')),
    is_business_shaped  boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    created_by uuid NOT NULL, updated_by uuid NOT NULL,
    row_version bytea NOT NULL,
    deleted_at timestamptz NULL
);
CREATE INDEX ix_dreams_user ON dreams(user_id) WHERE deleted_at IS NULL;

CREATE TABLE dream_statements (
    id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    dream_id     uuid NOT NULL UNIQUE REFERENCES dreams(id),
    statement    text NOT NULL,
    purpose      text NULL,
    who_it_helps text NULL,
    problem      text NULL,
    outcome      text NULL,
    motivation   text NULL,
    impact       text NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE obstacles (
    id           uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    dream_id     uuid NOT NULL REFERENCES dreams(id),
    category     varchar(40) NOT NULL
        CHECK (category IN ('money','knowledge','skills','confidence','time',
            'network','technology','market','family','fear_of_failure','clarity')),
    description  text NOT NULL,
    severity     varchar(10) NOT NULL CHECK (severity IN ('low','medium','high')),
    approach     text NULL,
    first_action text NULL,
    resolved_at  timestamptz NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE goals (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    dream_id uuid NOT NULL REFERENCES dreams(id),
    horizon varchar(20) NOT NULL
        CHECK (horizon IN ('five_year','three_year','one_year')),
    statement text NOT NULL,
    target_date date NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    deleted_at timestamptz NULL
);

CREATE TABLE missions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    goal_id uuid NOT NULL REFERENCES goals(id),
    title varchar(200) NOT NULL,
    horizon varchar(20) NOT NULL DEFAULT 'ninety_day',
    target_date date NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    deleted_at timestamptz NULL
);

CREATE TABLE projects (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    mission_id uuid NOT NULL REFERENCES missions(id),
    title varchar(200) NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    deleted_at timestamptz NULL
);

CREATE TABLE actions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    project_id uuid NULL REFERENCES projects(id),
    dream_id uuid NOT NULL REFERENCES dreams(id),
    goal_id uuid NULL REFERENCES goals(id),
    title varchar(200) NOT NULL,
    description text NULL,
    priority varchar(10) NOT NULL DEFAULT 'medium'
        CHECK (priority IN ('low','medium','high')),
    difficulty varchar(10) NOT NULL DEFAULT 'medium'
        CHECK (difficulty IN ('easy','medium','hard')),
    estimated_minutes int NULL,
    expected_impact varchar(10) NOT NULL DEFAULT 'medium'
        CHECK (expected_impact IN ('low','medium','high')),
    due_date date NULL,
    status varchar(20) NOT NULL DEFAULT 'not_started'
        CHECK (status IN ('not_started','in_progress','completed','blocked','cancelled')),
    is_next_best_action boolean NOT NULL DEFAULT false,
    completed_at timestamptz NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    deleted_at timestamptz NULL
);
CREATE INDEX ix_actions_dream_status ON actions(dream_id, status);
CREATE UNIQUE INDEX ux_actions_one_next_best_per_user
    ON actions(dream_id) WHERE is_next_best_action AND deleted_at IS NULL;

CREATE TABLE milestones (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    dream_id uuid NOT NULL REFERENCES dreams(id),
    title varchar(200) NOT NULL,
    achieved_at timestamptz NULL,
    is_custom boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE experiments (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    dream_id uuid NOT NULL REFERENCES dreams(id),
    idea_description text NOT NULL,
    hypothesis text NOT NULL,
    success_criteria text NOT NULL,
    status varchar(20) NOT NULL DEFAULT 'planned'
        CHECK (status IN ('planned','running','completed')),
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE experiment_results (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    experiment_id uuid NOT NULL REFERENCES experiments(id),
    outcome varchar(20) NOT NULL
        CHECK (outcome IN ('validated','partially_validated','invalidated')),
    evidence text NULL,
    learning text NOT NULL,
    next_experiment_id uuid NULL REFERENCES experiments(id),
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE business_ideas (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    dream_id uuid NOT NULL UNIQUE REFERENCES dreams(id),
    problem text NULL, customer text NULL, value_proposition text NULL,
    solution text NULL, business_model text NULL, market text NULL,
    competitors text NULL, pricing text NULL, marketing text NULL,
    sales text NULL, operations text NULL, technology text NULL,
    financial_assumptions text NULL, risks text NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE business_validations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    business_idea_id uuid NOT NULL REFERENCES business_ideas(id),
    viability_estimate smallint NULL CHECK (viability_estimate BETWEEN 0 AND 100),
    strong_assumptions jsonb NULL,
    weak_assumptions jsonb NULL,
    unknowns jsonb NULL,
    recommended_experiments jsonb NULL,
    generated_by_ai boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE journal_entries (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES asp_net_users(id),
    dream_id uuid NULL REFERENCES dreams(id),
    entry_type varchar(20) NOT NULL
        CHECK (entry_type IN ('daily','weekly','lesson','win','failure','idea','gratitude','vision')),
    body text NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    deleted_at timestamptz NULL
);
CREATE INDEX ix_journal_user ON journal_entries(user_id, created_at DESC);

CREATE TABLE achievements (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES asp_net_users(id),
    dream_id uuid NULL REFERENCES dreams(id),
    kind varchar(50) NOT NULL,
    description varchar(300) NOT NULL,
    achieved_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE community_posts (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES asp_net_users(id),
    dream_id uuid NULL REFERENCES dreams(id),
    body text NOT NULL,
    visibility varchar(20) NOT NULL DEFAULT 'private'
        CHECK (visibility IN ('private','followers','community','public')),
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    deleted_at timestamptz NULL
);

CREATE TABLE comments (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id uuid NOT NULL REFERENCES community_posts(id),
    user_id uuid NOT NULL REFERENCES asp_net_users(id),
    body text NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    deleted_at timestamptz NULL
);

CREATE TABLE mentor_profiles (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL UNIQUE REFERENCES asp_net_users(id),
    expertise jsonb NOT NULL,             -- ['marketing','finance',...]
    years_experience int NULL,
    availability varchar(50) NULL,
    verification_status varchar(20) NOT NULL DEFAULT 'unverified'
        CHECK (verification_status IN ('unverified','pending','verified')),
    rating_avg numeric(3,2) NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE help_requests (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES asp_net_users(id),
    dream_id uuid NULL REFERENCES dreams(id),
    category varchar(30) NOT NULL
        CHECK (category IN ('business','marketing','technology','finance','sales',
            'design','career','operations','leadership')),
    title varchar(200) NOT NULL,
    body text NOT NULL,
    status varchar(20) NOT NULL DEFAULT 'open'
        CHECK (status IN ('open','answered','closed')),
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE notifications (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES asp_net_users(id),
    kind varchar(50) NOT NULL,
    payload jsonb NOT NULL,
    read_at timestamptz NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX ix_notifications_user_unread ON notifications(user_id) WHERE read_at IS NULL;

CREATE TABLE ai_conversations (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id uuid NOT NULL REFERENCES asp_net_users(id),
    dream_id uuid NULL REFERENCES dreams(id),
    topic varchar(50) NOT NULL,  -- 'coach','idea_studio','challenge_my_idea'
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE ai_messages (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    conversation_id uuid NOT NULL REFERENCES ai_conversations(id) ON DELETE CASCADE,
    role varchar(10) NOT NULL CHECK (role IN ('user','assistant','system')),
    content text NOT NULL,
    prompt_template_version varchar(20) NULL,
    token_count int NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX ix_ai_messages_conversation ON ai_messages(conversation_id, created_at);
```

## Concurrency & integrity

- Optimistic concurrency via `row_version` (Postgres `xmin` mapped by EF Core,
  or an explicit `bytea` token) on every mutable entity — prevents silent
  overwrite when a user edits the same Dream Statement in two tabs.
- `ux_actions_one_next_best_per_user` enforces the "Next Best Action is
  always singular and visible" UX rule at the database level, not just in
  application code.
- All child tables use `RESTRICT` on delete; application-level soft delete
  cascades intentionally (deleting a Dream soft-deletes its Goals/Actions via
  a domain event, not a DB cascade), so audit trails survive.

## Migration strategy

- Each module owns its own EF Core `DbContext` and migration history table
  (`__EFMigrationsHistory_<module>`), applied independently on startup by an
  `IStartupMigrator` per module — a module can ship a migration without
  touching another module's history.
- Migrations are checked into `src/Modules/<Module>/Infrastructure/Migrations`
  and applied via `dotnet ef database update` in CI before integration tests
  run; never `EnsureCreated()` outside local dev.
