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
}
