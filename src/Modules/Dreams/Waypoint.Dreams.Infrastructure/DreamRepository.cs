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

    public async Task<IReadOnlyList<Dream>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.Dreams.Where(d => d.DeletedAt == null).OrderByDescending(d => d.CreatedAt).ToListAsync(cancellationToken);
}
