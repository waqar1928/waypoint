using Waypoint.Actions.Domain;

namespace Waypoint.Actions.Application;

public interface IActionsRepository
{
    Task<IReadOnlyList<ActionItem>> GetForDreamAsync(Guid dreamId, ActionStatus? statusFilter, CancellationToken cancellationToken);
    Task<ActionItem?> GetByIdAsync(Guid actionId, CancellationToken cancellationToken);
    Task<ActionItem?> GetNextBestActionAsync(Guid dreamId, CancellationToken cancellationToken);
    Task AddAsync(ActionItem action, CancellationToken cancellationToken);
    Task SaveAsync(ActionItem action, CancellationToken cancellationToken);

    /// <summary>Atomically clears IsNextBestAction on every other action for this dream — enforces "only one at a time" alongside the DB's unique filtered index.</summary>
    Task ClearNextBestActionForDreamAsync(Guid dreamId, Guid exceptActionId, CancellationToken cancellationToken);
}
