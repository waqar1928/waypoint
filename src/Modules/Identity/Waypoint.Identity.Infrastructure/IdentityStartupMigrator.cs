using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Identity.Infrastructure;

internal sealed class IdentityStartupMigrator(WaypointIdentityDbContext dbContext) : IStartupMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
