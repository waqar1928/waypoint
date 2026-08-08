namespace Waypoint.Audit.Domain;

/// <summary>
/// Append-only audit trail row. Deliberately not a Waypoint.Common.Entity
/// subclass — audit rows are immutable facts, not mutable aggregates, so
/// they don't need UpdatedAt/UpdatedBy/concurrency handling.
/// </summary>
public sealed class AuditLogEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid? TenantId { get; init; }
    public required string EntityType { get; init; }
    public required Guid EntityId { get; init; }
    public required string Action { get; init; }
    public Guid? ActorUserId { get; init; }
    public string? PayloadRedacted { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
