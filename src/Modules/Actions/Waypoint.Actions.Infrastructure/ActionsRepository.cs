using Microsoft.EntityFrameworkCore;
using Waypoint.Actions.Application;
using Waypoint.Actions.Domain;

namespace Waypoint.Actions.Infrastructure;

public sealed class ActionsRepository(ActionsDbContext db) : IActionsRepository
{
    public async Task<IReadOnlyList<ActionItem>> GetForDreamAsync(
        Guid dreamId, ActionStatus? statusFilter, CancellationToken cancellationToken)
    {
        var query = db.Actions.AsNoTracking().Where(a => a.DreamId == dreamId && a.DeletedAt == null);
        if (statusFilter is not null)
        {
            query = query.Where(a => a.Status == statusFilter);
        }

        return await query
            .OrderByDescending(a => a.IsNextBestAction)
            .ThenBy(a => a.DueDate)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<ActionItem?> GetByIdAsync(Guid actionId, CancellationToken cancellationToken) =>
        db.Actions.SingleOrDefaultAsync(a => a.Id == actionId && a.DeletedAt == null, cancellationToken);

    public Task<ActionItem?> GetNextBestActionAsync(Guid dreamId, CancellationToken cancellationToken) =>
        db.Actions.SingleOrDefaultAsync(
            a => a.DreamId == dreamId && a.IsNextBestAction && a.DeletedAt == null, cancellationToken);

    public async Task AddAsync(ActionItem action, CancellationToken cancellationToken)
    {
        db.Actions.Add(action);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(ActionItem action, CancellationToken cancellationToken)
    {
        if (db.Entry(action).State == EntityState.Detached)
        {
            db.Actions.Update(action);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearNextBestActionForDreamAsync(Guid dreamId, Guid exceptActionId, CancellationToken cancellationToken)
    {
        await db.Actions
            .Where(a => a.DreamId == dreamId && a.IsNextBestAction && a.Id != exceptActionId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.IsNextBestAction, false), cancellationToken);
    }

    public async Task DeleteAllForDreamAsync(Guid dreamId, CancellationToken cancellationToken)
    {
        await db.Actions.Where(a => a.DreamId == dreamId).ExecuteDeleteAsync(cancellationToken);
    }
}
