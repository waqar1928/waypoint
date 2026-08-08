# Waypoint — API Contract (Phase 1)

Base URL: `/api/v1`. JSON over HTTPS. Auth via HttpOnly secure cookies
(session) issued by ASP.NET Core Identity — no tokens exposed to JS, which
closes off an XSS-driven token theft path.

Later phases add `/api/v1/dreams`, `/api/v1/goals`, `/api/v1/actions`,
`/api/v1/experiments`, `/api/v1/ai/*`, etc., following the same envelope and
error format defined here so the contract stays consistent as it grows.

## Conventions

- All list endpoints are paginated: `?page=1&pageSize=20`, response includes
  `{ items, page, pageSize, totalCount }`.
- All mutating endpoints require a valid antiforgery token
  (`X-CSRF-TOKEN` header, double-submit cookie pattern) since auth is
  cookie-based.
- Rate limiting: 100 req/min per user on authenticated endpoints, 10 req/min
  per IP on unauthenticated auth endpoints (register/login/reset).

## Error envelope (RFC 7807 Problem Details)

```json
{
  "type": "https://waypoint.app/errors/validation-failed",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more fields are invalid.",
  "errors": {
    "email": ["Email is not a valid address."]
  },
  "traceId": "00-4bf9...-01"
}
```

## Auth & Identity

### `POST /api/v1/auth/register`
Request:
```json
{ "email": "user@example.com", "password": "•••••••••", "displayName": "Alex Rivera" }
```
Response `201`:
```json
{ "userId": "b3f1...", "email": "user@example.com", "emailConfirmationSent": true }
```
Errors: `400` validation (weak password, malformed email), `409` email already registered
(returns generic message to avoid account enumeration).

### `POST /api/v1/auth/login`
Request: `{ "email": "...", "password": "..." }`
Response `200`: `{ "userId": "...", "email": "...", "onboardingCompleted": false }`
Sets HttpOnly session cookie. `401` on bad credentials (generic message),
`423` if locked out after repeated failures.

### `POST /api/v1/auth/logout`
`204`, clears session cookie.

### `POST /api/v1/auth/verify-email`
Request: `{ "userId": "...", "token": "..." }` → `200` or `400 invalid/expired token`.

### `POST /api/v1/auth/forgot-password`
Request: `{ "email": "..." }` → always `202` regardless of whether the email
exists (prevents enumeration); sends reset email if it does.

### `POST /api/v1/auth/reset-password`
Request: `{ "userId": "...", "token": "...", "newPassword": "..." }` → `200` / `400`.

### `GET /api/v1/auth/session`
Returns current session info or `401` if not authenticated. Used by the
frontend on load to decide login vs. app shell.

## Profile

### `GET /api/v1/me/profile`
`200`:
```json
{
  "userId": "b3f1...",
  "displayName": "Alex Rivera",
  "bio": null,
  "avatarUrl": null,
  "timeZone": "America/New_York",
  "locale": "en-US",
  "onboardingCompletedAt": null
}
```

### `PUT /api/v1/me/profile`
Request: partial-update body of the editable fields above. `200` with the
updated resource, `400` on validation failure (e.g. `displayName` > 120
chars, invalid IANA time zone).

### `GET /api/v1/me/notification-preferences` / `PUT /api/v1/me/notification-preferences`
Boolean toggles as modeled in `users_notification_preferences`.

### `GET /api/v1/me/privacy-settings` / `PUT /api/v1/me/privacy-settings`
`profileVisibility` / `dreamVisibility` enums as modeled in
`users_privacy_settings`.

### `DELETE /api/v1/me`
Request: `{ "password": "..." }` (re-auth required for destructive
account action). `202` — queues erasure job; `200` response confirms
scheduling, actual purge happens async and is logged to `audit_log`.

## Health & ops (unauthenticated, used by orchestration)

### `GET /health/live` — process is up. `200 { "status": "Healthy" }`
### `GET /health/ready` — DB + dependent services reachable. `200`/`503`.

## Versioning policy

- URL-segment versioning (`/api/v1/...`). Breaking changes ship as `/v2`
  behind a deprecation window; additive changes (new optional fields) do not
  bump the version.
