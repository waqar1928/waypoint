using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Mentorship.Infrastructure;

internal sealed class MentorshipStartupMigrator(MentorshipDbContext dbContext) : IStartupMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
