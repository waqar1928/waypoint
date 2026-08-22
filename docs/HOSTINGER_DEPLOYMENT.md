# Hostinger VPS Deployment

Step-by-step guide to deploying Drevia on a Hostinger VPS using Docker and Docker Compose.

**Honesty note, matching this project's own standard (see
[DISASTER_RECOVERY.md](DISASTER_RECOVERY.md)):** this document is a real, actionable procedure,
written before this app has ever been deployed anywhere. Steps that involve a live server are
marked so you can follow them exactly; nothing in this document should be read as "already done."

Related documents: [ENVIRONMENT_VARIABLES.md](ENVIRONMENT_VARIABLES.md) (every variable in
detail), [DISASTER_RECOVERY.md](DISASTER_RECOVERY.md) (backup strategy this guide's Step 19
instantiates for Hostinger specifically), [BRAND_VOICE.md](BRAND_VOICE.md) (not relevant to
deployment, listed for completeness).

---

## 1. Create/prepare the Hostinger VPS

1. In Hostinger's hPanel, create a new VPS plan. Any plan with at least 2 vCPU / 4 GB RAM is a
   reasonable starting point for this stack (Postgres + a .NET API + a Next.js server + Caddy);
   size up if you expect real load before load-testing it yourself.
2. Pick a datacenter region close to your expected users.
3. **Do not** pick a plan-level "1-click app" for Postgres, Node, or .NET specifically — you want
   the OS-level Docker template described in the next step, not a pre-installed app stack that
   would conflict with the Compose-managed containers this guide sets up.

## 2. Use the Docker VPS template

Hostinger's VPS setup wizard offers an OS template list. Choose the **Docker** template (Ubuntu
with Docker Engine and Docker Compose pre-installed) rather than a bare OS image — this avoids a
manual Docker install step and matches what Hostinger's own Docker Manager panel expects to find.
If you pick a bare OS image instead, install Docker Engine + the Compose plugin yourself
(`https://docs.docker.com/engine/install/ubuntu/`) before continuing.

## 3. Connect to the VPS

Hostinger's hPanel shows the VPS's public IPv4 address and lets you set an initial root password
or upload an SSH public key at creation time. Prefer the SSH key option.

```bash
ssh root@<VPS_IP>
```

## 4. Configure SSH

Do this before anything else — the VPS is on the public internet from the moment it boots.

1. Create a non-root user with sudo access rather than operating as `root` day-to-day:
   ```bash
   adduser deploy
   usermod -aG sudo deploy
   usermod -aG docker deploy   # so `deploy` can run docker/docker compose without sudo
   ```
2. Copy your SSH public key to the new user:
   ```bash
   rsync --archive --chown=deploy:deploy ~/.ssh /home/deploy
   ```
3. Harden `sshd` (`/etc/ssh/sshd_config`): set `PasswordAuthentication no` and
   `PermitRootLogin no`, then `systemctl restart sshd`. Confirm you can still log in as `deploy`
   with your key **in a second terminal, before closing your first session** — if the key-based
   login is broken, you want your original root session still open to fix it.
4. Enable a firewall (Hostinger's hPanel has one, or use `ufw` on the VPS itself):
   allow 22 (SSH), 80 (HTTP), 443 (HTTPS). Nothing else needs to be open — Postgres and the API
   are never exposed outside the Docker network (see `docker-compose.prod.yml`).

From here on, connect as `ssh deploy@<VPS_IP>`.

## 5. Clone the private repository

```bash
sudo apt-get update && sudo apt-get install -y git
ssh-keygen -t ed25519 -C "deploy@drevia-vps"   # then add the printed public key as a GitHub
                                                 # deploy key (read-only) on this repository
git clone git@github.com:<your-org>/waypoint.git /opt/apps/drevia
cd /opt/apps/drevia
```

Using a GitHub **deploy key** (repo-scoped, read-only) rather than a personal access token or your
own SSH key keeps the VPS's access limited to exactly this one repository.

## 6. Configure production environment

```bash
cp .env.production.example .env
chmod 600 .env          # only the deploy user can read it
nano .env                # or vim/your editor of choice
```

Fill in every value marked `CHANGE_ME` — see the next step for how to generate each one.

## 7. Supply secrets securely

Every secret this app needs is described in [ENVIRONMENT_VARIABLES.md](ENVIRONMENT_VARIABLES.md).
None of them are pasted into chat, committed to git, or baked into a Docker image — they live only
in `/opt/apps/drevia/.env` on the VPS (mode `600`, owned by `deploy`), which
`docker-compose.prod.yml` reads at container-start time.

| Secret | How to get it |
|---|---|
| `POSTGRES_PASSWORD` | Generate on the VPS: `openssl rand -base64 32`. Never reuse the local dev password. |
| `ANTHROPIC_API_KEY` | From your Anthropic Console account (console.anthropic.com) → API Keys. Optional — leave blank to launch with Drevia Coach disabled. |
| `SMTP_HOST` / `SMTP_USERNAME` / `SMTP_PASSWORD` | From whichever SMTP relay or transactional-email provider you choose. Optional — leave blank to launch with emails logged instead of sent. |
| `VAPID_PUBLIC_KEY` / `VAPID_PRIVATE_KEY` | **Required** — the API will not start without both. Generate a fresh keypair, once, outside this repo and outside any chat session: `npx web-push generate-vapid-keys` (Node/npm is already required on this VPS to build the `web` container, so this needs no new toolchain). Copy both the public and private key from the SAME command's output — see [ENVIRONMENT_VARIABLES.md](ENVIRONMENT_VARIABLES.md)'s Push Notifications section for why a mismatched pair fails silently at delivery time rather than at startup. Never reuse a keypair generated for local development. |
| `VAPID_SUBJECT` | **Required.** A real, monitored `mailto:` address (e.g. `mailto:you@drevia.net`) or `https://` URL for push services to contact you about delivery problems. Not generated — you choose this value; a placeholder is not acceptable even though the app doesn't reject one. |

`POSTGRES_DB` and `POSTGRES_USER` aren't secrets in the same sense, but pick real values (not the
dev defaults) rather than leaving them as `CHANGE_ME`. `WEB_APP_BASE_URL` should be
`https://app.drevia.net` once DNS is live (Step 11); you can deploy with the plain IP or a
`localhost`-style placeholder first and update it once DNS resolves, then redeploy.

Unlike `ANTHROPIC_API_KEY`/SMTP, the VAPID variables are **not optional** — the container will not
start without them once `docker-compose.prod.yml`'s `api.environment:` block requires all three
(see Step 8's `config` check below, which will fail loudly with a clear "Set VAPID_PUBLIC_KEY in
.env" message if any is missing). If you genuinely want to launch without push notifications for
now, do not run `npx web-push generate-vapid-keys` casually — see whether disabling the
Notifications module's push wiring entirely is a better fit than shipping a made-up keypair; this
guide assumes push notifications are wanted, since they're a shipped feature as of this release.

## 8. Configure Docker Compose and the shared reverse proxy

`docker-compose.prod.yml` (already in the repo) is the file this deployment uses — it is
deliberately separate from `docker-compose.yml` (local dev only; publishes Postgres/API ports and
serves plain HTTP, neither of which belongs in production).

### The reverse proxy lives outside this repo, on purpose

This repo does **not** contain a Caddyfile and `docker-compose.prod.yml` does **not** define a
Caddy service. Both used to, and that was a mistake worth understanding before you reproduce it:
the VPS also serves an unrelated site, whose vhost had been hand-added to the Caddyfile *inside
this repo's tracked tree*. The deploy workflow ran `git reset --hard`, which silently discards
local edits to tracked files — so that config was one deploy away from deletion. Worse, Caddy only
reads its config at startup, so nothing would have broken until the next restart, arbitrarily far
from the deploy that caused it.

Shared infrastructure therefore lives in its own Compose project, outside every application repo:

```
~/caddy/
  docker-compose.yml     project name: caddy; owns ports 80/443 and the TLS volumes
  Caddyfile              one line: import /etc/caddy/sites/*.caddy
  sites/
    drevia.caddy         this app's vhosts
    <other>.caddy        one file per unrelated site
```

Each application attaches to a shared external network, `web_proxy`. No application repo can
affect another's proxy config, and no application deploy can restart or reconfigure the proxy.

Create the network once (it must exist before Step 9 — `docker-compose.prod.yml` declares it
`external: true` and will refuse to start without it):

```bash
docker network create web_proxy
```

Then create `~/caddy/docker-compose.yml`:

```yaml
name: caddy
services:
  caddy:
    image: caddy:2-alpine
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./Caddyfile:/etc/caddy/Caddyfile:ro
      - ./sites:/etc/caddy/sites:ro
      - caddy_data:/data
      - caddy_config:/config
    networks:
      - web_proxy
networks:
  web_proxy:
    external: true
volumes:
  caddy_data:
  caddy_config:
```

`~/caddy/Caddyfile` is a single line, so adding a site never means editing a shared file:

```
import /etc/caddy/sites/*.caddy
```

And `~/caddy/sites/drevia.caddy` — `web` is the service name from `docker-compose.prod.yml`,
resolved by Docker DNS over `web_proxy`:

```
# Drevia
drevia.net, www.drevia.net, app.drevia.net {
	header {
		Strict-Transport-Security "max-age=31536000; includeSubDomains"
		X-Content-Type-Options "nosniff"
		X-Frame-Options "DENY"
		Referrer-Policy "strict-origin-when-cross-origin"
	}

	reverse_proxy web:3000

	encode gzip

	log {
		output stdout
		format json
	}
}
```

Those response headers are set here rather than in the application because they belong to every
response the origin serves, including error pages the Next.js server never sees. Don't drop them
when adapting this for another site. Note `Strict-Transport-Security` instructs browsers to refuse
plain HTTP for a full year — correct once TLS works, but don't enable it on a hostname whose
certificate you haven't confirmed, since you cannot un-send it to browsers that cached it.

Start it before the application:

```bash
cd ~/caddy && docker compose up -d
```

### Review the application config

```bash
cd /opt/apps/drevia
docker compose -f docker-compose.prod.yml config
```

This resolves and prints the fully-interpolated config without starting anything — a good way to
confirm your `.env` values are being picked up correctly (secrets included, so don't paste this
output anywhere).

## 9. Start the application

```bash
docker compose -f docker-compose.prod.yml up -d --build
```

First run builds the `api` and `web` images (several minutes), pulls `postgres:16-alpine`, and
starts everything. Caddy is **not** part of this project — it was started separately in Step 8 and
keeps running across application deploys, which is the point: redeploying the app never restarts
the proxy or interrupts TLS for any other site sharing it. Watch it come up:

```bash
docker compose -f docker-compose.prod.yml ps
docker compose -f docker-compose.prod.yml logs -f
```

## 10. Run database migrations safely

`Waypoint__AutoMigrate=true` is set in `docker-compose.prod.yml`, so the `api` container applies
any pending EF Core migrations automatically on startup (see `Program.cs`'s `IStartupMigrator`
loop) — this is the same mechanism local dev already uses, not a new production-only step. Every
migration in this codebase's history is additive-only (no destructive migration has ever been
written); see [DISASTER_RECOVERY.md](DISASTER_RECOVERY.md)'s "Bad migration deployed" row for the
policy on what to do if a genuinely destructive migration is ever needed.

**Before every deploy that includes a new migration**, take a fresh backup first (Step 19) — auto
-migrate is safe by policy, not by magic, and a backup taken 30 seconds before a migration runs is
cheap insurance. Watch the `api` container's logs during startup to confirm migrations applied
cleanly:

```bash
docker compose -f docker-compose.prod.yml logs api | grep -i migrat
```

## 11. Configure domain DNS

Do not assume this is already done. In whatever registrar/DNS provider manages `drevia.net` (which
may or may not be Hostinger itself, depending on where the domain is registered), create:

| Type | Name | Value | Purpose |
|---|---|---|---|
| A | `@` (drevia.net) | `<VPS_IP>` | Marketing/root domain |
| A | `www` | `<VPS_IP>` | `www.drevia.net` |
| A | `app` | `<VPS_IP>` | `app.drevia.net`, the primary application URL |

All three point at the same VPS IP — this is one Next.js app serving both the marketing pages and
the authenticated app, not three separate deployments (see `~/caddy/sites/drevia.caddy`). If you use
Hostinger's own DNS zone editor instead of an external one, the equivalent A records are created
there.

DNS propagation can take anywhere from a few minutes to 24-48 hours depending on your registrar's
TTL and resolver caching. Check propagation with `dig drevia.net` / `dig app.drevia.net` from
outside the VPS, or a public tool, before moving to the HTTPS step — Caddy's automatic certificate
request (next step) will keep retrying and failing noisily until DNS actually resolves to this
server.

## 12. Configure HTTPS

Nothing to do manually here beyond DNS being correct. The Caddy container (started in Step 8, in
its own project) requests and renews Let's Encrypt certificates automatically for every hostname
listed in `~/caddy/sites/*.caddy`, the first time it sees a real request for that hostname with
working DNS.

Certificate state persists in the `caddy_caddy_data` volume (Compose prefixes volume names with the
project name, so the `caddy_data:` volume in a project named `caddy` becomes `caddy_caddy_data` —
verify with `docker volume ls` rather than assuming). A container restart therefore doesn't mean a
re-issue.

That volume also holds the ACME account key, which is why it must be **copied, never moved or
recreated**, if you ever relocate the proxy. Let's Encrypt enforces rate limits per hostname —
losing this volume means re-issuing every certificate at once, and a busy VPS can exhaust the
weekly limit and be left without working TLS for days.

Confirm it worked:

```bash
curl -I https://drevia.net
curl -I https://app.drevia.net
```

Both should return real HTTP headers over a valid TLS connection, not a certificate warning.
Certificate validation is never disabled anywhere in this stack — if `curl` complains about the
cert, that's a real problem to fix (usually: DNS not propagated yet, or port 80/443 not reachable
from the internet), not something to suppress.

## 13. Verify health endpoints

```bash
# From the VPS itself (internal network):
docker compose -f docker-compose.prod.yml exec api curl -fsS http://localhost:8080/health/live
docker compose -f docker-compose.prod.yml exec api curl -fsS http://localhost:8080/health/ready
```

`/health/live` confirms the process is up. `/health/ready` additionally confirms real Postgres
connectivity (it's wired to `AddNpgSql`, not a synthetic check) — a `200` here means the api
container can actually reach the database, not just that it started.

## 14. Verify the frontend

```bash
curl -I https://drevia.net
curl -I https://app.drevia.net/login
```

Then load `https://drevia.net` in a real browser: confirm the landing page renders with Drevia
branding, no mixed-content warnings, and the browser's dev tools console/network tab show no
errors.

## 15. Verify the API

The API has no public port (by design — see the network topology comment at the top of
`docker-compose.prod.yml`), so it's only reachable through the web app's BFF proxy. Verify it end
-to-end instead of directly: register a real test account through `https://app.drevia.net/register`
and confirm you receive a `201`-equivalent success in the UI, not a "Drevia's API is currently
unreachable" error (see `apps/web/src/lib/proxy.ts`).

## 16. Verify PostgreSQL

```bash
docker compose -f docker-compose.prod.yml exec postgres pg_isready -U "$POSTGRES_USER" -d "$POSTGRES_DB"
docker compose -f docker-compose.prod.yml exec postgres psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -c "\dt users.*"
```

Confirm tables exist (migrations applied) and, from outside the VPS, confirm the database is
genuinely unreachable: `nc -zv <VPS_IP> 5432` from your own machine should fail to connect —
if it succeeds, something is wrong (Postgres should never be exposed publicly; there is no
`ports:` entry for it in `docker-compose.prod.yml`, but this is worth confirming for real on a
live server rather than trusting the config alone).

## 17. Verify email

If you configured SMTP in Step 7: trigger a real password-reset request from the login page and
confirm the email actually arrives. If you left SMTP unset: trigger the same flow and confirm the
API logs the email instead of erroring —

```bash
docker compose -f docker-compose.prod.yml logs api | grep -i "email (dev mode"
```

— a log line appearing there with no error is the expected, intentional behavior when SMTP isn't
configured yet, not a bug.

## 18. Verify AI Coach

If you configured `ANTHROPIC_API_KEY`: log in, open Coach, and confirm you get a real response.
If you left it unset: confirm Coach shows a clear "not configured yet" message rather than a raw
500 error or a hung request.

## 19. Configure backups

This instantiates [DISASTER_RECOVERY.md](DISASTER_RECOVERY.md)'s "fallback approach: manual
`pg_dump`" section (the recommended path there, managed-provider point-in-time recovery, doesn't
apply here since this is self-hosted Postgres in a container) for this specific VPS.

**Backup script** (`/opt/apps/drevia/backup.sh`):

```bash
#!/usr/bin/env bash
set -euo pipefail
cd /opt/apps/drevia
source .env
BACKUP_DIR=/opt/apps/drevia-backups
mkdir -p "$BACKUP_DIR"
TIMESTAMP=$(date -u +%Y%m%dT%H%M%SZ)
docker compose -f docker-compose.prod.yml exec -T postgres \
  pg_dump -U "$POSTGRES_USER" --format=custom --compress=9 "$POSTGRES_DB" \
  > "$BACKUP_DIR/drevia-$TIMESTAMP.dump"
# Retention: keep the last 7 daily dumps on-disk; see "off-VPS copy" note below for real durability.
find "$BACKUP_DIR" -name "drevia-*.dump" -mtime +7 -delete
```

```bash
chmod +x /opt/apps/drevia/backup.sh
chmod 700 /opt/apps/drevia-backups
```

**Frequency:** nightly, via cron:

```bash
crontab -e
# Add:
0 3 * * * /opt/apps/drevia/backup.sh >> /opt/apps/drevia-backups/backup.log 2>&1
```

**Location:** `/opt/apps/drevia-backups` on the VPS by default. Per
[DISASTER_RECOVERY.md](DISASTER_RECOVERY.md), a backup on the same disk as the database it backs
up protects against a bad `DELETE`/`DROP` but not against VPS-level disk failure — copy dumps to
off-VPS object storage (Hostinger Object Storage, S3-compatible, or similar) on a schedule as a
follow-up hardening step once the basic on-VPS rotation above is confirmed working.

**Retention:** 7 daily dumps on-VPS (via the `find -mtime +7 -delete` line above). Extend to the
full grandfather-father-son rotation described in DISASTER_RECOVERY.md once dumps are also
copied off-VPS.

**Restore procedure:** follow
[DISASTER_RECOVERY.md § "From a manual pg_dump backup"](DISASTER_RECOVERY.md#from-a-manual-pg_dump-backup)
exactly, substituting `docker compose -f docker-compose.prod.yml exec postgres` in front of each
`psql`/`pg_restore`/`createdb` command (or `docker compose ... exec -T postgres` when piping a
file in, as in the backup script above) since Postgres runs inside a container here rather than
being reachable from the host directly.

**This has not been tested.** Per this project's own standard (see DISASTER_RECOVERY.md's opening
note and its still-open "Restore drill" section), do not treat this backup setup as trustworthy
until you have actually run a full restore drill against a real dump from this VPS: take a backup,
restore it to a scratch database, point a throwaway copy of the app at it, and confirm real data
comes back correctly. Until that drill has been run once for real, this is a documented plan, not
a proven safety net.

## 20. Configure monitoring

Nothing sophisticated is set up by default — treat this as a starting point, not a complete
observability stack (a full APM/alerting platform is a paid-infrastructure decision outside this
guide's scope, same reasoning as DISASTER_RECOVERY.md's stance on managed Postgres).

- **Container health:** `docker compose -f docker-compose.prod.yml ps` shows each service's
  `Dockerfile`-defined `HEALTHCHECK` status at a glance.
- **Logs:** `docker compose -f docker-compose.prod.yml logs -f [service]`. The API logs
  structured JSON in Production (see `Program.cs`'s Serilog setup) — pipe through `jq` for
  readability: `docker compose -f docker-compose.prod.yml logs api | jq .`
- **Resource usage:** Hostinger's hPanel shows VPS-level CPU/RAM/disk graphs. `docker stats` on
  the VPS itself shows per-container usage.
- **Uptime:** point an external uptime checker (even a free one) at
  `https://app.drevia.net/api/csrf` or similar lightweight endpoint, since `/health/*` isn't
  publicly reachable (the API has no public port by design — see Step 15).

## 21. Update/redeploy procedure

Prefer the GitHub Actions workflow (`.github/workflows/deploy.yml`, manual trigger only) — it runs
the same commands plus preflight checks. To do it by hand:

```bash
cd /opt/apps/drevia
git pull --ff-only origin main
docker compose -f docker-compose.prod.yml up -d --build
```

`--ff-only` is deliberate. The workflow previously ran `git reset --hard origin/main`, which
silently discards local edits to tracked files — including, at one point, config for an unrelated
site. `--ff-only` refuses and reports instead of destroying. If it refuses, something on the VPS
diverged from `main`; investigate what and why before forcing past it.

The workflow also refuses to deploy when `git status --porcelain --untracked-files=no` is non-empty.
Uncommitted changes to tracked files on a production host mean the running config isn't the config
in the repo, and no deploy should paper over that. Fix by committing the change upstream and
pulling it, not by discarding it blindly.

Compose only rebuilds and recreates containers whose image actually changed, so a deploy where
only `apps/web` changed doesn't restart `postgres` or interrupt open database connections. Caddy is
in a different project entirely and is never touched by this command. Take a backup first (Step 19's
script) if the deploy includes a migration, per Step 10.

## 22. Rollback procedure

```bash
cd /opt/apps/drevia
git log --oneline -5                 # find the last known-good commit
git checkout <previous-good-commit>
docker compose -f docker-compose.prod.yml up -d --build
```

This leaves the repo on a **detached HEAD**, which the deploy workflow's preflight deliberately
rejects (it requires `main`). That is intended: while rolled back, automated deploys should fail
loudly rather than quietly fast-forward you off a rollback you're still investigating. Return to
`main` when the fix is ready, as described below.

If the problem is data-level (a bad migration already ran, not just bad application code), rolling
back the code does not undo the migration — restore from the most recent pre-deploy backup
instead, per Step 19's restore procedure and DISASTER_RECOVERY.md's "Bad migration deployed" row.
After rolling back code only (no data restore needed), return to `main`:
`git checkout main && git pull --ff-only`.

### Rolling back the reverse proxy

The proxy is a separate concern with a separate rollback. It has its own Compose project and its own
TLS volumes, so an application rollback never touches it — and vice versa. If a Caddy change breaks
routing, edit the relevant file under `~/caddy/sites/` and reload:

```bash
cd ~/caddy
docker compose exec caddy caddy validate --config /etc/caddy/Caddyfile   # check before applying
docker compose restart caddy
```

Validate first. A syntax error means Caddy fails to start, which takes down every site it serves —
not just the one you were editing.

---

## What this document deliberately does not do

- **Does not execute any of these steps.** Everything above is written for you to run on your own
  VPS with your own credentials — see the project's own rule that production secrets are never
  pasted into chat or handled by an assistant directly.
- **Does not claim backups are tested.** They aren't yet — see Step 19.
- **Does not stand up monitoring/alerting beyond what's described in Step 20.** A real alerting
  pipeline (PagerDuty, Grafana alerting, etc.) is a follow-up decision, not assumed here.
