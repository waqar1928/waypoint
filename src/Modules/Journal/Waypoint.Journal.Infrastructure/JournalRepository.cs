using Microsoft.EntityFrameworkCore;
using Waypoint.Journal.Application;
using Waypoint.Journal.Domain;

namespace Waypoint.Journal.Infrastructure;

public sealed class JournalRepository(JournalDbContext db) : IJournalRepository
{
    public async Task AddAsync(JournalEntry entry, CancellationToken cancellationToken)
    {
        db.JournalEntries.Add(entry);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JournalEntry>> GetRecentForUserAsync(
        Guid userId, int take, CancellationToken cancellationToken) =>
        await db.JournalEntries
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.DeletedAt == null)
            .OrderByDescending(e => e.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
}
