namespace Waypoint.Notifications.Domain;

/// <summary>
/// A single in-app notification for one recipient. Deliberately not a Waypoint.Common.Entity
/// subclass — like AuditLogEntry, this is closer to an append-only fact than a mutable aggregate;
/// the only mutation it ever undergoes is IsRead flipping true, which doesn't need
/// UpdatedBy/concurrency handling since only the recipient themselves can ever change it.
/// </summary>
public sealed class Notification
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid RecipientUserId { get; init; }
    public required string Category { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public string? LinkUrl { get; init; }
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
