using Waypoint.Experiments.Domain;

namespace Waypoint.Experiments.Application;

public interface IExperimentsRepository
{
    Task<IReadOnlyList<Experiment>> GetForDreamAsync(Guid dreamId, CancellationToken cancellationToken);
    Task<Experiment?> GetByIdAsync(Guid experimentId, CancellationToken cancellationToken);
    Task AddAsync(Experiment experiment, CancellationToken cancellationToken);
    Task SaveAsync(Experiment experiment, CancellationToken cancellationToken);

    Task<IReadOnlyList<ExperimentResult>> GetResultsForExperimentsAsync(
        IReadOnlyList<Guid> experimentIds, CancellationToken cancellationToken);

    Task AddResultAsync(ExperimentResult result, CancellationToken cancellationToken);

    /// <summary>Real hard delete of every Experiment and its Results for one dream — backs
    /// account deletion (see docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section).
    /// ExperimentResult has no DB-level FK/cascade from Experiment (see
    /// ExperimentResultConfiguration — only an index on ExperimentId), so this deletes results
    /// explicitly rather than relying on the database to clean them up.</summary>
    Task DeleteAllForDreamAsync(Guid dreamId, CancellationToken cancellationToken);
}
