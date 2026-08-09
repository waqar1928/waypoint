using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Community.Infrastructure;

internal sealed class CommunityStartupMigrator(CommunityDbContext dbContext) : IStartupMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
