using Waypoint.Journal.Domain;

namespace Waypoint.Journal.Application;

public interface IJournalRepository
{
    Task AddAsync(JournalEntry entry, CancellationToken cancellationToken);
    Task<IReadOnlyList<JournalEntry>> GetRecentForUserAsync(Guid userId, int take, CancellationToken cancellationToken);

    /// <summary>Real hard delete, not filtered on DeletedAt — backs account deletion (see
    /// docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section). Journal entries are always
    /// private (no sharing mechanism exists anywhere in this codebase), so this is the only place
    /// they can ever be removed once written — soft-deleting would leave the actual private
    /// content sitting in the database forever, defeating the point of deleting an account.</summary>
    Task DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken);
}
