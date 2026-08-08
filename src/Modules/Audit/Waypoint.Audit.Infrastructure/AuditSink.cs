using Waypoint.Audit.Domain;
using Waypoint.Common;

namespace Waypoint.Audit.Infrastructure;

public sealed class AuditSink(AuditDbContext db) : IAuditSink
{
    public async Task RecordAsync(AuditEntry entry, CancellationToken cancellationToken)
    {
        db.AuditLogEntries.Add(new AuditLogEntry
        {
            EntityType = entry.EntityType,
            EntityId = entry.EntityId,
            Action = entry.Action,
            ActorUserId = entry.ActorUserId,
            PayloadRedacted = entry.PayloadRedacted,
            OccurredAt = entry.OccurredAt,
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
