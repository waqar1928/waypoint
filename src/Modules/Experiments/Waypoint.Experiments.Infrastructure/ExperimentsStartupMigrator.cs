using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Experiments.Infrastructure;

internal sealed class ExperimentsStartupMigrator(ExperimentsDbContext dbContext) : IStartupMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
