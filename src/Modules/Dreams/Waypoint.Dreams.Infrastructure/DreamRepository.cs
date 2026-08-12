using Microsoft.EntityFrameworkCore;
using Waypoint.Dreams.Application;
using Waypoint.Dreams.Domain;

namespace Waypoint.Dreams.Infrastructure;

public sealed class DreamRepository(DreamsDbContext db) : IDreamRepository
{
    public Task<Dream?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Dreams.Include(d => d.Statement)
            .SingleOrDefaultAsync(d => d.UserId == userId && d.DeletedAt == null, cancellationToken);

    public Task<bool> ExistsForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Dreams.AnyAsync(d => d.UserId == userId && d.DeletedAt == null, cancellationToken);

    public async Task SaveAsync(Dream dream, CancellationToken cancellationToken)
    {
        if (db.Entry(dream).State == EntityState.Detached)
        {
            db.Dreams.Add(dream);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Dream>> GetAllAsync(int take, CancellationToken cancellationToken) =>
        await db.Dreams.AsNoTracking().Where(d => d.DeletedAt == null).OrderByDescending(d => d.CreatedAt).Take(take).ToListAsync(cancellationToken);

    public async Task DeleteForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Deliberately not filtered on DeletedAt == null — a user with an already-soft-deleted
        // dream should still have it actually purged on account deletion, not left behind because
        // it wouldn't match a "still active" filter.
        await db.Dreams.Where(d => d.UserId == userId).ExecuteDeleteAsync(cancellationToken);
    }
}
