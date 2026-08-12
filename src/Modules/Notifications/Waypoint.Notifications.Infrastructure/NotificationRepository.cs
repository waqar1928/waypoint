using Microsoft.EntityFrameworkCore;
using Waypoint.Notifications.Application;
using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Infrastructure;

public sealed class NotificationRepository(NotificationsDbContext db) : INotificationRepository
{
    public async Task<IReadOnlyList<Notification>> GetForUserAsync(Guid userId, int take, CancellationToken cancellationToken) =>
        await db.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientUserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Notifications.AsNoTracking().CountAsync(n => n.RecipientUserId == userId && !n.IsRead, cancellationToken);

    public Task<Notification?> GetByIdAsync(Guid notificationId, CancellationToken cancellationToken) =>
        db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken);

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken)
    {
        db.Notifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task SaveAsync(Notification notification, CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);

    public Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Notifications
            .Where(n => n.RecipientUserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(n => n.IsRead, true).SetProperty(n => n.ReadAt, DateTimeOffset.UtcNow),
                cancellationToken);

    public Task DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Notifications.Where(n => n.RecipientUserId == userId).ExecuteDeleteAsync(cancellationToken);
}
