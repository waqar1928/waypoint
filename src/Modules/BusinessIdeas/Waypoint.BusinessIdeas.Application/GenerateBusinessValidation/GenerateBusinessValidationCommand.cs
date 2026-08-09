using MediatR;
using Waypoint.Common;
using Waypoint.BusinessIdeas.Domain;

namespace Waypoint.BusinessIdeas.Application.GenerateBusinessValidation;

public sealed record GenerateBusinessValidationCommand : IRequest<BusinessValidationDto>;

public sealed class GenerateBusinessValidationCommandHandler(
    IBusinessIdeasRepository repository, IDreamSummaryProvider dreamSummaryProvider,
    ICurrentUserAccessor currentUser, IViabilityEstimateGenerator generator)
    : IRequestHandler<GenerateBusinessValidationCommand, BusinessValidationDto>
{
    public async Task<BusinessValidationDto> Handle(GenerateBusinessValidationCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        var idea = await repository.GetForDreamAsync(dream.DreamId, cancellationToken)
            ?? throw new NotFoundException("Start your business profile before generating a viability estimate.");

        var draft = generator.Generate(idea, dream.Title);
        var validation = BusinessValidation.Create(
            idea.Id, userId, draft.ViabilityEstimate, draft.StrongAssumptions, draft.WeakAssumptions,
            draft.Unknowns, draft.RecommendedExperiments, generatedByAi: true);

        await repository.AddValidationAsync(validation, cancellationToken);

        return BusinessValidationDto.From(validation);
    }
}
