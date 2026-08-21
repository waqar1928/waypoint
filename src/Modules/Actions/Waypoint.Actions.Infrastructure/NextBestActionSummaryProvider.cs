using Waypoint.Actions.Application;
using Waypoint.Actions.Application.GetNextBestAction;
using Waypoint.Common;

namespace Waypoint.Actions.Infrastructure;

/// <summary>
/// Implements the cross-module INextBestActionSummaryProvider read contract — see
/// Waypoint.Common/PushNotifications.cs. Deliberately mirrors GetNextBestActionQueryHandler's
/// exact logic (pin wins, otherwise NextBestActionSelector.SelectFrom(...)) rather than the worker
/// calling that handler directly: GetNextBestActionQueryHandler reads the acting user from
/// ICurrentUserAccessor (an HTTP-request concept the background worker has no equivalent of - it's
/// evaluating many users, not "the current user"), so this takes userId as a parameter instead.
/// The one thing that must never drift between the two call sites is the actual recommendation
/// algorithm - both call the exact same NextBestActionSelector.SelectFrom static method, and the
/// pin-rationale copy comes from the same NextBestActionSelector.PinnedRationale constant, so
/// there is exactly one place either could ever be wrong.
/// </summary>
public sealed class NextBestActionSummaryProvider(
    IActionsRepository repository, IDreamSummaryProvider dreamSummaryProvider, TimeProvider timeProvider)
    : INextBestActionSummaryProvider
{
    public async Task<NextBestActionSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);
        if (dream is null)
        {
            return null;
        }

        var pinned = await repository.GetNextBestActionAsync(dream.DreamId, cancellationToken);
        if (pinned is not null)
        {
            return new NextBestActionSummary(pinned.Id, pinned.Title, NextBestActionSelector.PinnedRationale);
        }

        var openActions = await repository.GetForDreamAsync(dream.DreamId, statusFilter: null, cancellationToken);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var picked = NextBestActionSelector.SelectFrom(openActions, today);
        return picked is null
            ? null
            : new NextBestActionSummary(picked.Value.Action.Id, picked.Value.Action.Title, picked.Value.Rationale);
    }
}
