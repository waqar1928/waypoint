using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Application.Push;

/// <summary>Deliberately excludes P256dhKey/AuthKey - those never need to leave the server once
/// submitted (the browser already has its own copy), and are treated as sensitive. Endpoint IS
/// included since the frontend needs it to identify "is my current subscription already
/// registered" without re-deriving it, but the worker/logging paths still never log it in full.</summary>
public sealed record PushSubscriptionDto(
    Guid Id, string? UserAgent, DateTimeOffset CreatedAt, DateTimeOffset LastSeenAt, string Status)
{
    public static PushSubscriptionDto From(PushSubscription s) =>
        new(s.Id, s.UserAgent, s.CreatedAt, s.LastSeenAt, s.Status.ToString());
}
