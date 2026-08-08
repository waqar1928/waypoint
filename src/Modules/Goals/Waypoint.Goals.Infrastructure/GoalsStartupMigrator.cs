using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Goals.Infrastructure;

internal sealed class GoalsStartupMigrator(GoalsDbContext dbContext) : IStartupMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
