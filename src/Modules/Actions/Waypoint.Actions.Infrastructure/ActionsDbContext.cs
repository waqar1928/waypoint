using Microsoft.EntityFrameworkCore;
using Waypoint.Actions.Domain;

namespace Waypoint.Actions.Infrastructure;

public sealed class ActionsDbContext(DbContextOptions<ActionsDbContext> options) : DbContext(options)
{
    public DbSet<ActionItem> Actions => Set<ActionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ActionsDbContext).Assembly);
    }
}
