using Microsoft.EntityFrameworkCore;
using Waypoint.Experiments.Domain;

namespace Waypoint.Experiments.Infrastructure;

public sealed class ExperimentsDbContext(DbContextOptions<ExperimentsDbContext> options) : DbContext(options)
{
    public DbSet<Experiment> Experiments => Set<Experiment>();
    public DbSet<ExperimentResult> ExperimentResults => Set<ExperimentResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExperimentsDbContext).Assembly);
    }
}
