using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Notifications.Infrastructure;

internal sealed class NotificationsStartupMigrator(NotificationsDbContext dbContext) : IStartupMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
