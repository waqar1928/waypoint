using Microsoft.EntityFrameworkCore;
using Waypoint.Goals.Domain;

namespace Waypoint.Goals.Infrastructure;

public sealed class GoalsDbContext(DbContextOptions<GoalsDbContext> options) : DbContext(options)
{
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<Milestone> Milestones => Set<Milestone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GoalsDbContext).Assembly);
    }
}
