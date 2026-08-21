using Microsoft.EntityFrameworkCore;
using Waypoint.Notifications.Application.Push;
using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Infrastructure;

public sealed class PushSubscriptionRepository(NotificationsDbContext db) : IPushSubscriptionRepository
{
    public async Task<PushSubscription> UpsertAsync(
        Guid userId, string endpoint, string p256dhKey, string authKey, string? userAgent,
        CancellationToken cancellationToken)
    {
        var existing = await db.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint, cancellationToken);
        if (existing is not null)
        {
            // Reactivates a browser that re-subscribed (permission re-granted after being
            // revoked), and correctly re-associates the endpoint if a different Drevia account
            // signs into the same browser later - a push endpoint belongs to one browser+origin
            // registration, and the most recent subscriber is the correct owner.
            existing.LastSeenAt = DateTimeOffset.UtcNow;
            existing.Status = PushSubscriptionStatus.Active;
            existing.DeactivatedAt = null;
            existing.DeactivatedReason = null;
            existing.ConsecutiveFailureCount = 0;
            await db.SaveChangesAsync(cancellationToken);
            return existing;
        }

        var created = PushSubscription.Create(userId, endpoint, p256dhKey, authKey, userAgent);
        db.PushSubscriptions.Add(created);
        await db.SaveChangesAsync(cancellationToken);
        return created;
    }

    public async Task<IReadOnlyList<PushSubscription>> GetActiveForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.PushSubscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.Status == PushSubscriptionStatus.Active)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PushSubscription>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.PushSubscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastSeenAt)
            .ToListAsync(cancellationToken);

    public Task<PushSubscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.PushSubscriptions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task DeactivateAsync(PushSubscription subscription, string reason, CancellationToken cancellationToken)
    {
        subscription.Status = PushSubscriptionStatus.Deactivated;
        subscription.DeactivatedAt = DateTimeOffset.UtcNow;
        subscription.DeactivatedReason = reason;
        db.PushSubscriptions.Update(subscription);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordSuccessAsync(PushSubscription subscription, CancellationToken cancellationToken)
    {
        subscription.LastSuccessAt = DateTimeOffset.UtcNow;
        subscription.LastSeenAt = DateTimeOffset.UtcNow;
        subscription.ConsecutiveFailureCount = 0;
        db.PushSubscriptions.Update(subscription);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordFailureAsync(PushSubscription subscription, CancellationToken cancellationToken)
    {
        subscription.LastFailureAt = DateTimeOffset.UtcNow;
        subscription.ConsecutiveFailureCount += 1;
        db.PushSubscriptions.Update(subscription);
        await db.SaveChangesAsync(cancellationToken);
    }
}
