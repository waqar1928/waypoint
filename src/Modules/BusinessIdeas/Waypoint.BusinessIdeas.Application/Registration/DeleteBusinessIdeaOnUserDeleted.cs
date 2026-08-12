using MediatR;
using Waypoint.Common;

namespace Waypoint.BusinessIdeas.Application.Registration;

/// <summary>
/// Cascade-deletes this module's data when an account is deleted — see
/// docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section. BusinessIdea (and its
/// Validations) key off DreamId (never UserId directly), so this uses the event's snapshotted
/// DreamId — see UserDeletedIntegrationEvent's doc comment for why. No-ops if the user never
/// completed onboarding.
/// </summary>
public sealed class DeleteBusinessIdeaOnUserDeleted(IBusinessIdeasRepository repository)
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    public Task Handle(UserDeletedIntegrationEvent notification, CancellationToken cancellationToken) =>
        notification.DreamId is { } dreamId
            ? repository.DeleteForDreamAsync(dreamId, cancellationToken)
            : Task.CompletedTask;
}
