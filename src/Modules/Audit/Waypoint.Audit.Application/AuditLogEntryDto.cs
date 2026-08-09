using Waypoint.Audit.Domain;

namespace Waypoint.Audit.Application;

public sealed record AuditLogEntryDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    Guid? ActorUserId,
    string? PayloadRedacted,
    DateTimeOffset OccurredAt)
{
    public static AuditLogEntryDto From(AuditLogEntry entry) => new(
        entry.Id, entry.EntityType, entry.EntityId, entry.Action, entry.ActorUserId, entry.PayloadRedacted, entry.OccurredAt);
}
