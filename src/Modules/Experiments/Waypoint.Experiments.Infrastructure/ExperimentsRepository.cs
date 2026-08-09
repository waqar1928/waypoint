using Microsoft.EntityFrameworkCore;
using Waypoint.Experiments.Application;
using Waypoint.Experiments.Domain;

namespace Waypoint.Experiments.Infrastructure;

public sealed class ExperimentsRepository(ExperimentsDbContext db) : IExperimentsRepository
{
    public async Task<IReadOnlyList<Experiment>> GetForDreamAsync(Guid dreamId, CancellationToken cancellationToken) =>
        await db.Experiments.AsNoTracking().Where(e => e.DreamId == dreamId && e.DeletedAt == null)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<Experiment?> GetByIdAsync(Guid experimentId, CancellationToken cancellationToken) =>
        db.Experiments.SingleOrDefaultAsync(e => e.Id == experimentId && e.DeletedAt == null, cancellationToken);

    public async Task AddAsync(Experiment experiment, CancellationToken cancellationToken)
    {
        db.Experiments.Add(experiment);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(Experiment experiment, CancellationToken cancellationToken)
    {
        if (db.Entry(experiment).State == EntityState.Detached)
        {
            db.Experiments.Update(experiment);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExperimentResult>> GetResultsForExperimentsAsync(
        IReadOnlyList<Guid> experimentIds, CancellationToken cancellationToken)
    {
        if (experimentIds.Count == 0)
        {
            return [];
        }

        return await db.ExperimentResults
            .AsNoTracking()
            .Where(r => experimentIds.Contains(r.ExperimentId))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddResultAsync(ExperimentResult result, CancellationToken cancellationToken)
    {
        db.ExperimentResults.Add(result);
        await db.SaveChangesAsync(cancellationToken);
    }
}
