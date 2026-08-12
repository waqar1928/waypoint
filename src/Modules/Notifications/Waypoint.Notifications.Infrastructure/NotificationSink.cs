using Waypoint.Common;
using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Infrastructure;

public sealed class NotificationSink(NotificationsDbContext db) : INotificationSink
{
    public async Task SendAsync(NotificationToSend notification, CancellationToken cancellationToken)
    {
        db.Notifications.Add(new Notification
        {
            RecipientUserId = notification.RecipientUserId,
            Category = notification.Category,
            Title = notification.Title,
            Body = notification.Body,
            LinkUrl = notification.LinkUrl,
            CreatedAt = notification.OccurredAt,
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
