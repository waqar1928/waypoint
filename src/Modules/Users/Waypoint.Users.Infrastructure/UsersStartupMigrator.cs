using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Users.Infrastructure;

internal sealed class UsersStartupMigrator(UsersDbContext dbContext) : IStartupMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
