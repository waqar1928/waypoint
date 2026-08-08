using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Journal.Infrastructure;

internal sealed class JournalStartupMigrator(JournalDbContext dbContext) : IStartupMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
