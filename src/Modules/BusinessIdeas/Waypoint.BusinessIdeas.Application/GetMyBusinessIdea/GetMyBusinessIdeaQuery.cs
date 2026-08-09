using MediatR;
using Waypoint.Common;

namespace Waypoint.BusinessIdeas.Application.GetMyBusinessIdea;

public sealed record GetMyBusinessIdeaQuery : IRequest<BusinessIdeaDto?>;

public sealed class GetMyBusinessIdeaQueryHandler(
    IBusinessIdeasRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyBusinessIdeaQuery, BusinessIdeaDto?>
{
    public async Task<BusinessIdeaDto?> Handle(GetMyBusinessIdeaQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);
        if (dream is null)
        {
            return null;
        }

        var idea = await repository.GetForDreamAsync(dream.DreamId, cancellationToken);
        return idea is null ? null : BusinessIdeaDto.From(idea);
    }
}
