using Microsoft.EntityFrameworkCore;
using Waypoint.Common;
using Waypoint.Dreams.Domain;

namespace Waypoint.Dreams.Infrastructure;

/// <summary>Implements the cross-module IDreamSummaryProvider read contract — see docs/03-domain-model.md.</summary>
public sealed class DreamSummaryProvider(DreamsDbContext db) : IDreamSummaryProvider
{
    public async Task<DreamSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var dream = await db.Dreams.Include(d => d.Statement)
            .SingleOrDefaultAsync(d => d.UserId == userId && d.DeletedAt == null, cancellationToken);

        return dream is null ? null : ToSummary(dream);
    }

    public async Task<DreamSummary?> GetByIdAsync(Guid dreamId, CancellationToken cancellationToken)
    {
        var dream = await db.Dreams.Include(d => d.Statement)
            .SingleOrDefaultAsync(d => d.Id == dreamId && d.DeletedAt == null, cancellationToken);

        return dream is null ? null : ToSummary(dream);
    }

    public async Task<IReadOnlyDictionary<Guid, DreamSummary>> GetByIdsAsync(
        IReadOnlyList<Guid> dreamIds, CancellationToken cancellationToken)
    {
        if (dreamIds.Count == 0)
        {
            return new Dictionary<Guid, DreamSummary>();
        }

        var dreams = await db.Dreams.Include(d => d.Statement)
            .Where(d => dreamIds.Contains(d.Id) && d.DeletedAt == null)
            .ToListAsync(cancellationToken);

        return dreams.ToDictionary(d => d.Id, ToSummary);
    }

    private static DreamSummary ToSummary(Dream dream) => new(
        dream.Id, dream.UserId, dream.Title, dream.Statement.Statement, dream.Statement.Purpose,
        dream.Statement.WhoItHelps, dream.Statement.Problem, dream.Statement.Outcome,
        dream.Statement.Motivation, dream.Statement.Impact, dream.IsBusinessShaped);
}
