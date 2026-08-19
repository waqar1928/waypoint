using MediatR;
using Waypoint.Common;

namespace Waypoint.Actions.Application.GetNextBestAction;

public sealed record GetNextBestActionQuery : IRequest<ActionDto?>;

/// <summary>
/// A pin the user set explicitly (SetNextBestActionCommand) always wins, since that's a deliberate
/// choice they made. Otherwise this falls back to NextBestActionSelector's computed pick over every
/// open action for the dream, so "next best action" means something even before anyone has ever
/// clicked "Make this next" - and re-picks automatically once the current one is done, instead of
/// going empty (see UpdateActionStatusCommand, which clears the pin on completion/cancellation).
/// </summary>
public sealed class GetNextBestActionQueryHandler(
    IActionsRepository repository, IDreamSummaryProvider dreamSummaryProvider,
    ICurrentUserAccessor currentUser, TimeProvider timeProvider)
    : IRequestHandler<GetNextBestActionQuery, ActionDto?>
{
    public async Task<ActionDto?> Handle(GetNextBestActionQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);
        if (dream is null)
        {
            return null;
        }

        var pinned = await repository.GetNextBestActionAsync(dream.DreamId, cancellationToken);
        if (pinned is not null)
        {
            return ActionDto.From(pinned, "You marked this as your next move.");
        }

        var openActions = await repository.GetForDreamAsync(dream.DreamId, statusFilter: null, cancellationToken);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var picked = NextBestActionSelector.SelectFrom(openActions, today);
        return picked is null ? null : ActionDto.From(picked.Value.Action, picked.Value.Rationale);
    }
}
