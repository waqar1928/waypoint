using Waypoint.Journal.Domain;

namespace Waypoint.Journal.Application;

public interface IJournalRepository
{
    Task AddAsync(JournalEntry entry, CancellationToken cancellationToken);
    Task<IReadOnlyList<JournalEntry>> GetRecentForUserAsync(Guid userId, int take, CancellationToken cancellationToken);
}
