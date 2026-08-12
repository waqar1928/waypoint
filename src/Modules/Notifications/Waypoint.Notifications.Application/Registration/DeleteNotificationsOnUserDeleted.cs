using MediatR;
using Waypoint.Common;

namespace Waypoint.Notifications.Application.Registration;

/// <summary>
/// Cascade-deletes this module's data when an account is deleted — see
/// docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section, and
/// INotificationRepository.DeleteAllForUserAsync's doc comment for exactly what gets removed.
/// Notification keys directly off RecipientUserId, so this doesn't need the event's DreamId.
/// </summary>
public sealed class DeleteNotificationsOnUserDeleted(INotificationRepository repository)
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    public Task Handle(UserDeletedIntegrationEvent notification, CancellationToken cancellationToken) =>
        repository.DeleteAllForUserAsync(notification.UserId, cancellationToken);
}
