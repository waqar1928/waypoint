using Waypoint.Actions.Application;
using Waypoint.Actions.Domain;
using Waypoint.Common;

namespace Waypoint.Actions.Infrastructure;

/// <summary>Implements the cross-module IActionsSummaryProvider read contract — see Waypoint.Common/Auditing.cs.</summary>
public sealed class ActionsSummaryProvider(IActionsRepository repository, IDreamSummaryProvider dreamSummaryProvider)
    : IActionsSummaryProvider
{
    // Bounded on purpose - this feeds an AI prompt, not a page. Not-started/in-progress actions
    // are what's actually relevant to "what am I working on"; completed/blocked/cancelled ones
    // aren't current state. The Next Best Action always sorts first when present.
    private const int MaxItems = 6;

    public async Task<ActionsSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);
        if (dream is null)
        {
            return null;
        }

        var actions = await repository.GetForDreamAsync(dream.DreamId, null, cancellationToken);
        var relevant = actions
            .Where(a => a.Status is ActionStatus.NotStarted or ActionStatus.InProgress)
            .OrderByDescending(a => a.IsNextBestAction)
            .ThenByDescending(a => a.CreatedAt)
            .Take(MaxItems)
            .Select(a => new ActionSummaryItem(a.Title, a.Status.ToString(), a.IsNextBestAction))
            .ToList();

        return new ActionsSummary(relevant);
    }
}
