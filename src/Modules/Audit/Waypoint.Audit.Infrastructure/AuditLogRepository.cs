using Microsoft.EntityFrameworkCore;
using Waypoint.Audit.Application;
using Waypoint.Audit.Domain;

namespace Waypoint.Audit.Infrastructure;

public sealed class AuditLogRepository(AuditDbContext db) : IAuditLogRepository
{
    public async Task<IReadOnlyList<AuditLogEntry>> GetRecentAsync(int take, CancellationToken cancellationToken) =>
        await db.AuditLogEntries
            .AsNoTracking()
            .OrderByDescending(e => e.OccurredAt)
            .Take(take)
            .ToListAsync(cancellationToken);
}
