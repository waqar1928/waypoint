using MediatR;
using Waypoint.Common;

namespace Waypoint.Goals.Application.Registration;

/// <summary>
/// Cascade-deletes this module's data when an account is deleted — see
/// docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section. Goal, Mission, and Milestone all
/// key off DreamId (never UserId directly), so this uses the event's snapshotted DreamId rather
/// than a live IDreamSummaryProvider lookup — see UserDeletedIntegrationEvent's doc comment for
/// why a live lookup during cascade-delete handling would be unsafe. No-ops if the user never
/// completed onboarding (DreamId is null — nothing here to delete).
/// </summary>
public sealed class DeletePlanOnUserDeleted(IGoalsRepository repository)
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    public Task Handle(UserDeletedIntegrationEvent notification, CancellationToken cancellationToken) =>
        notification.DreamId is { } dreamId
            ? repository.DeleteAllForDreamAsync(dreamId, cancellationToken)
            : Task.CompletedTask;
}
