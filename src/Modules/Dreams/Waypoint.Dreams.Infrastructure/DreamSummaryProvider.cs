using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Dreams.Infrastructure;

/// <summary>Implements the cross-module IDreamSummaryProvider read contract — see docs/03-domain-model.md.</summary>
public sealed class DreamSummaryProvider(DreamsDbContext db) : IDreamSummaryProvider
{
    public async Task<DreamSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var dream = await db.Dreams.Include(d => d.Statement)
            .SingleOrDefaultAsync(d => d.UserId == userId && d.DeletedAt == null, cancellationToken);

        if (dream is null) return null;

        return new DreamSummary(
            dream.Id, dream.UserId, dream.Title, dream.Statement.Statement, dream.Statement.Purpose,
            dream.Statement.WhoItHelps, dream.Statement.Problem, dream.Statement.Outcome,
            dream.Statement.Motivation, dream.Statement.Impact, dream.IsBusinessShaped);
    }
}
