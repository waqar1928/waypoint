namespace Waypoint.Common;

/// <summary>
/// Role name constants shared between the Identity module (which seeds/assigns them) and every
/// other module's Application layer (which needs the exact string to check IsInRole without
/// depending on Identity.Infrastructure). See docs/07-technical-architecture.md: "policy-based
/// RBAC... admin surface is a fully separate policy set, never reachable by a regular user role
/// even if a route is guessed."
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";

    /// <summary>
    /// Scoped to the moderation queue (dismiss/remove/resolve reports) and mentor verification
    /// only — everything else under /api/v1/admin/* (user lock/unlock, dream oversight, AI usage,
    /// the full audit log) still requires the full Admin role. See
    /// docs/PRODUCTION_READINESS_AUDIT.md's Authorization section: previously every admin had
    /// full platform power with no least-privilege separation. Same config-seeded, no-self-service
    /// bootstrap as Admin (see RoleSeeder) — Waypoint:ModeratorEmails.
    /// </summary>
    public const string Moderator = "Moderator";
}
