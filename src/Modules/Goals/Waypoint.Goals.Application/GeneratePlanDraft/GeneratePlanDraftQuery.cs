using MediatR;
using Waypoint.Common;

namespace Waypoint.Goals.Application.GeneratePlanDraft;

public sealed record GeneratePlanDraftQuery : IRequest<PlanDraftDto>;

public sealed class GeneratePlanDraftQueryHandler(
    IDreamSummaryProvider dreamSummaryProvider, IPlanDraftGenerator generator, ICurrentUserAccessor currentUser)
    : IRequestHandler<GeneratePlanDraftQuery, PlanDraftDto>
{
    public async Task<PlanDraftDto> Handle(GeneratePlanDraftQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        return generator.Generate(dream);
    }
}
