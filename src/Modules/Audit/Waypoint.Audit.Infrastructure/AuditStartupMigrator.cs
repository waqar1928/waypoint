using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Audit.Infrastructure;

internal sealed class AuditStartupMigrator(AuditDbContext dbContext) : IStartupMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
