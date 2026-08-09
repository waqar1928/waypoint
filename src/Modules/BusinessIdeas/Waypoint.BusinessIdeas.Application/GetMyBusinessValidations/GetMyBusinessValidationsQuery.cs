using MediatR;
using Waypoint.Common;

namespace Waypoint.BusinessIdeas.Application.GetMyBusinessValidations;

public sealed record GetMyBusinessValidationsQuery : IRequest<IReadOnlyList<BusinessValidationDto>>;

public sealed class GetMyBusinessValidationsQueryHandler(
    IBusinessIdeasRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyBusinessValidationsQuery, IReadOnlyList<BusinessValidationDto>>
{
    public async Task<IReadOnlyList<BusinessValidationDto>> Handle(GetMyBusinessValidationsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);
        if (dream is null)
        {
            return [];
        }

        var idea = await repository.GetForDreamAsync(dream.DreamId, cancellationToken);
        if (idea is null)
        {
            return [];
        }

        var validations = await repository.GetValidationsForIdeaAsync(idea.Id, cancellationToken);
        return validations.Select(BusinessValidationDto.From).ToList();
    }
}
