using MediatR;
using Waypoint.Common;

namespace Waypoint.AI.Application.Registration;

/// <summary>
/// Cascade-deletes this module's data when an account is deleted — see
/// docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section. AiConversation keys directly off
/// UserId, so this doesn't need the event's DreamId. AiMessage cascades from AiConversation at the
/// DB level (see IAiRepository.DeleteAllForUserAsync's doc comment).
/// </summary>
public sealed class DeleteConversationsOnUserDeleted(IAiRepository repository)
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    public Task Handle(UserDeletedIntegrationEvent notification, CancellationToken cancellationToken) =>
        repository.DeleteAllForUserAsync(notification.UserId, cancellationToken);
}
