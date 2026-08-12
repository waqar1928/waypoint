using MediatR;
using Waypoint.Common;

namespace Waypoint.Mentorship.Application.Registration;

/// <summary>
/// Cascade-deletes this module's data when an account is deleted — see
/// docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section, and
/// IMentorshipRepository.DeleteAllForUserAsync's doc comment for exactly what gets removed.
/// MentorProfile/HelpRequest/HelpRequestResponse all key directly off UserId (or
/// ResponderUserId), so this doesn't need the event's DreamId.
/// </summary>
public sealed class DeleteMentorshipDataOnUserDeleted(IMentorshipRepository repository)
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    public Task Handle(UserDeletedIntegrationEvent notification, CancellationToken cancellationToken) =>
        repository.DeleteAllForUserAsync(notification.UserId, cancellationToken);
}
