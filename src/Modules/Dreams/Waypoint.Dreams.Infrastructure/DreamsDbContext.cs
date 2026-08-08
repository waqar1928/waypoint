using Microsoft.EntityFrameworkCore;
using Waypoint.Dreams.Domain;

namespace Waypoint.Dreams.Infrastructure;

public sealed class DreamsDbContext(DbContextOptions<DreamsDbContext> options) : DbContext(options)
{
    public DbSet<Dream> Dreams => Set<Dream>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DreamsDbContext).Assembly);
    }
}
