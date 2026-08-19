using MediatR;
using Waypoint.Actions.Domain;
using Waypoint.Common;

namespace Waypoint.Actions.Application.GetMyActions;

public sealed record GetMyActionsQuery(ActionStatus? StatusFilter) : IRequest<IReadOnlyList<ActionDto>>;

public sealed class GetMyActionsQueryHandler(
    IActionsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyActionsQuery, IReadOnlyList<ActionDto>>
{
    public async Task<IReadOnlyList<ActionDto>> Handle(GetMyActionsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);
        if (dream is null)
        {
            return [];
        }

        var actions = await repository.GetForDreamAsync(dream.DreamId, request.StatusFilter, cancellationToken);
        // Explicit lambda, not a bare `ActionDto.From` method-group reference: now that From has an
        // optional second parameter, the method group is ambiguous against Select's two overloads
        // (Func<T,R> vs Func<T,int,R>) and fails to compile.
        return actions.Select(a => ActionDto.From(a)).ToList();
    }
}
