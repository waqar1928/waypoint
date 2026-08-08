using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Dreams.Infrastructure;

internal sealed class DreamsStartupMigrator(DreamsDbContext dbContext) : IStartupMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
