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
}
