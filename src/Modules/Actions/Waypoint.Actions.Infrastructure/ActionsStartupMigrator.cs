using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Actions.Infrastructure;

internal sealed class ActionsStartupMigrator(ActionsDbContext dbContext) : IStartupMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
