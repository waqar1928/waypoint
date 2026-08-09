using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.AI.Infrastructure;

internal sealed class AiStartupMigrator(AiDbContext dbContext) : IStartupMigrator
{
    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
        await PromptTemplateSeeder.SeedAsync(dbContext, cancellationToken);
    }
}
