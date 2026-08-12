# Waypoint — Disaster Recovery & Backup Plan

Date: 2026-08-10. Written as part of the production-readiness pass documented
in `docs/PRODUCTION_READINESS_AUDIT.md`.

**Honesty note up front, per this project's own standard:** this document
describes a real, actionable *procedure*. It is not a substitute for
actually running that procedure against a real deployed environment and
timing it. No such environment exists yet (this project has never been
deployed anywhere — see `docs/PRODUCTION_CHECKLIST.md`). Every step below
that hasn't been executed against a real system is marked **UNVERIFIED**.
Once this is deployed for real, the single most important next action is to
run the "Restore drill" section below for real, on a real backup, and update
this document with the actual result — not assume it works because it reads
correctly.

---

## What's actually at risk

Every piece of user-generated content in this application lives in one
Postgres database: dreams, journal entries (always private, never
recoverable from anywhere else if lost), goals/missions/milestones,
experiments, business plans, community posts/comments, mentorship
history, AI conversation history, and account/identity data. There is no
secondary copy of any of it anywhere else in the system. **A single
uncontained Postgres failure or operator mistake (`DROP TABLE`, a bad
migration, accidental `DELETE` without a `WHERE` clause) can permanently
destroy real users' private journal and business-planning data with no
recovery path unless backups exist and actually work.**

---

## Recovery objectives

These are targets to design toward, not measured facts about a system that
doesn't exist yet:

- **RPO (Recovery Point Objective): 24 hours**, tightening to as low as the
  hosting provider's continuous/point-in-time-recovery window allows once a
  real managed Postgres provider is chosen (see below) — most managed
  Postgres offerings support point-in-time recovery to any second within a
  retention window, which is a materially better RPO than a nightly dump and
  should be preferred over building custom backup infrastructure.
- **RTO (Recovery Time Objective): 4 hours** for a full database restore
  from backup on a fresh instance, assuming a chosen host's restore
  tooling — **UNVERIFIED**, no restore has been timed.

---

## Backup strategy

### Recommended approach: managed Postgres point-in-time recovery

If Waypoint is deployed on a managed Postgres provider (AWS RDS, Azure
Database for PostgreSQL, Google Cloud SQL, Railway, Render, Supabase, Neon,
etc. — the actual choice is a paid-infrastructure decision this document
deliberately doesn't make, per this project's own stop-list), use that
provider's built-in continuous backup / point-in-time recovery (PITR)
feature rather than building custom backup tooling. Every major managed
Postgres offering has one, it's continuously tested by the provider (not
by this project), and it gives second-level RPO instead of the
whole-hours RPO a manual nightly dump gives. **This is the recommended
path and the one to actually use** — the manual `pg_dump` approach below
exists as a documented fallback for self-hosted deployments where no
managed PITR is available, not as the primary plan.

Action item for whoever deploys this for real: when choosing a host,
confirm PITR/automated backups are available and turn them on explicitly
— most providers don't enable it by default on the cheapest tier.

### Fallback approach: manual `pg_dump` (self-hosted Postgres only)

If Waypoint is self-hosted (e.g. the `docker-compose.yml` Postgres
container, or a bare-metal/VM Postgres instance with no managed backup
feature), back up with `pg_dump` on a schedule:

```bash
# Full logical backup, compressed, timestamped.
pg_dump \
  --host="$PGHOST" --port="$PGPORT" --username=waypoint \
  --format=custom --compress=9 \
  --file="waypoint-$(date -u +%Y%m%dT%H%M%SZ).dump" \
  waypoint
```

- **Schedule:** nightly, via cron or the hosting platform's scheduled-job
  feature. Nightly gives a 24-hour RPO in the worst case — acceptable for a
  first launch, worth tightening once real usage volume justifies more
  frequent backups.
- **Retention:** keep 7 daily backups + 4 weekly backups + 3 monthly
  backups (standard grandfather-father-son rotation). Delete older backups
  automatically to bound storage cost.
- **Storage:** copy each dump to object storage (S3-compatible) in a
  different failure domain than the database itself — a backup sitting on
  the same disk/volume as the database it backs up isn't a real backup, it
  protects against nothing but a bad `DELETE` statement (which is still
  worth protecting against, but isn't the whole threat model).
- **Encryption:** the dump file contains full user data (journal entries,
  business plans) — encrypt at rest in object storage (most S3-compatible
  storage supports this natively; enable it) and restrict access to backup
  storage to the smallest possible set of credentials.

### What's explicitly out of scope for backups

- **AI conversation content** stored in Postgres (via the `AI` module) is
  backed up along with everything else above — no special handling needed,
  it's just more rows in the same database.
- **No secrets are ever stored in the database** (confirmed in
  `docs/PRODUCTION_READINESS_AUDIT.md` section on data protection) — backups
  of the app database never need special secret-redaction handling.

---

## Restore procedure

### From managed-provider PITR

Follow the specific provider's console/CLI restore-to-point-in-time flow
(mechanically different per provider, but conceptually: pick a timestamp,
the provider spins up a new instance restored to that point, then you
repoint the application's `ConnectionStrings__Postgres` at the new
instance). **UNVERIFIED** — no provider has been chosen yet, so no
provider-specific runbook exists yet. Whoever picks a host should write the
exact console steps here.

### From a manual `pg_dump` backup

```bash
# 1. Create a fresh, empty database to restore into (never restore over a
#    live database in place — always restore to a new target, verify it,
#    then cut the application over).
createdb --host="$PGHOST" --username=waypoint waypoint_restored

# 2. Restore the dump into it.
pg_restore \
  --host="$PGHOST" --port="$PGPORT" --username=waypoint \
  --dbname=waypoint_restored \
  --no-owner --no-privileges \
  waypoint-20260810T030000Z.dump

# 3. Verify row counts on a few key tables look sane before cutting over
#    (compare against the last known-good count if you have one, or at
#    minimum confirm they're non-zero and roughly proportional to each
#    other — e.g. more journal entries than users, not the reverse).
psql --host="$PGHOST" --username=waypoint --dbname=waypoint_restored \
  -c "SELECT count(*) FROM users.\"AspNetUsers\";" \
  -c "SELECT count(*) FROM dreams.dreams;"

# 4. Only after verification: repoint ConnectionStrings__Postgres at
#    waypoint_restored (or rename databases) and restart the API.
```

**This procedure has never been executed against a real backup in this
project. UNVERIFIED.** The commands are correct standard `pg_dump`/
`pg_restore` usage, but "the commands are correct" and "this actually
restores a working Waypoint database" are different claims — only the
second one matters, and only a real drill proves it.

---

## Restore drill (run this for real before depending on it)

1. Take a real backup of a real (or realistic staging) database using the
   procedure above.
2. Actually restore it to a fresh database, following the steps above
   exactly as written.
3. Point a real running instance of the API at the restored database.
4. Log in as a real (test) user and confirm their dreams/journal/goals are
   present and correct.
5. Time the whole process from "decision to restore" to "application
   serving correctly from the restored database" — that duration is the
   real RTO, not the 4-hour target stated above.
6. Update this document with the actual timing and any steps that didn't
   work as written.

**Status: not yet run. This is the single most important unfinished item
in this document** — a backup strategy that has never been tested by
restoring from it is a hope, not a plan.

---

## Failure scenarios and response

| Scenario | Response |
|---|---|
| Application bug causes bad data for some users (not full DB loss) | Restore to a scratch database, extract only the affected rows, apply a targeted fix — do not do a full-database restore-and-cutover for a partial-data problem, that would roll back every other user's legitimate changes too |
| Full database loss/corruption | Full restore per the procedure above, from the most recent backup; accept up to the RPO's worth of data loss (whatever changed since that backup) |
| Bad migration deployed | EF Core migrations in this codebase are additive-by-default (no destructive migration has ever been written — verified via the migration history, every one is a pure schema addition). If a genuinely destructive migration is ever needed, it must ship as two separate deploys (stop writing to the old shape, backfill, then drop the old shape in a later release) — never a single migration that drops/renames a column in the same deploy that also changes application code to stop using it. This project's own stop-list already requires explicit user approval before any destructive migration runs, which is the first line of defense here |
| Accidental `DELETE`/`DROP` by an operator | Restore per the procedure above. This is exactly why backup storage must be separate from the live database — an operator with delete access to the live DB not automatically having delete access to backup storage is a real, worthwhile access-control separation |
| Region/host outage (not a data problem, an availability problem) | Out of scope for this document — that's a hosting/infrastructure redundancy decision (multi-region, standby replica) tied to which paid host is chosen, not something to design in the abstract before a host exists |

---

## What this document deliberately does not do

- **Does not pick a hosting provider.** That's a paid-infrastructure
  decision explicitly on this project's own stop-list requiring the user's
  sign-off.
- **Does not claim backups are currently running anywhere.** They aren't —
  nothing is deployed yet. This document is the plan for when something is.
- **Does not claim the restore procedure has been tested.** It hasn't. See
  the Restore Drill section.
