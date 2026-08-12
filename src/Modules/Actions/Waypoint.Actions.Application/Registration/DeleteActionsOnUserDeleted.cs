using MediatR;
using Waypoint.Common;

namespace Waypoint.Actions.Application.Registration;

/// <summary>
/// Cascade-deletes this module's data when an account is deleted — see
/// docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section. ActionItem keys off DreamId
/// (never UserId directly), so this uses the event's snapshotted DreamId — see
/// UserDeletedIntegrationEvent's doc comment for why. No-ops if the user never completed
/// onboarding.
/// </summary>
public sealed class DeleteActionsOnUserDeleted(IActionsRepository repository)
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    public Task Handle(UserDeletedIntegrationEvent notification, CancellationToken cancellationToken) =>
        notification.DreamId is { } dreamId
            ? repository.DeleteAllForDreamAsync(dreamId, cancellationToken)
            : Task.CompletedTask;
}
