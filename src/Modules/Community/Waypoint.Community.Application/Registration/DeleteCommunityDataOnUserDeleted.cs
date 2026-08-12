using MediatR;
using Waypoint.Common;

namespace Waypoint.Community.Application.Registration;

/// <summary>
/// Cascade-deletes this module's data when an account is deleted — see
/// docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section, and
/// ICommunityRepository.DeleteAllForUserAsync's doc comment for exactly what gets removed.
/// CommunityPost/Comment/ContentReportRecord all key directly off UserId (or ReporterUserId), so
/// this doesn't need the event's DreamId.
/// </summary>
public sealed class DeleteCommunityDataOnUserDeleted(ICommunityRepository repository)
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    public Task Handle(UserDeletedIntegrationEvent notification, CancellationToken cancellationToken) =>
        repository.DeleteAllForUserAsync(notification.UserId, cancellationToken);
}
