using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Application;

public sealed record NotificationDto(
    Guid Id,
    string Category,
    string Title,
    string Body,
    string? LinkUrl,
    bool IsRead,
    DateTimeOffset CreatedAt)
{
    public static NotificationDto From(Notification notification) => new(
        notification.Id,
        notification.Category,
        notification.Title,
        notification.Body,
        notification.LinkUrl,
        notification.IsRead,
        notification.CreatedAt);
}
