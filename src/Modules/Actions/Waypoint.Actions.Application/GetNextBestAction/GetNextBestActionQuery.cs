using MediatR;
using Waypoint.Common;

namespace Waypoint.Actions.Application.GetNextBestAction;

public sealed record GetNextBestActionQuery : IRequest<ActionDto?>;

public sealed class GetNextBestActionQueryHandler(
    IActionsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
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

        var action = await repository.GetNextBestActionAsync(dream.DreamId, cancellationToken);
        return action is null ? null : ActionDto.From(action);
    }
}
