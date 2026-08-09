using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.BusinessIdeas.Infrastructure;

internal sealed class BusinessIdeasStartupMigrator(BusinessIdeasDbContext dbContext) : IStartupMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken) =>
        dbContext.Database.MigrateAsync(cancellationToken);
}
