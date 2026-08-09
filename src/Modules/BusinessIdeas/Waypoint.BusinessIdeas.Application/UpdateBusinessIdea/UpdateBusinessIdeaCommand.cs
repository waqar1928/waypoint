using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.BusinessIdeas.Domain;

namespace Waypoint.BusinessIdeas.Application.UpdateBusinessIdea;

public sealed record UpdateBusinessIdeaCommand(
    string? Problem,
    string? Customer,
    string? ValueProposition,
    string? Solution,
    string? BusinessModel,
    string? Market,
    string? Competitors,
    string? Pricing,
    string? Marketing,
    string? Sales,
    string? Operations,
    string? Technology,
    string? FinancialAssumptions,
    string? Risks) : IRequest<BusinessIdeaDto>;

public sealed class UpdateBusinessIdeaCommandValidator : AbstractValidator<UpdateBusinessIdeaCommand>
{
    public UpdateBusinessIdeaCommandValidator()
    {
        RuleFor(x => x.Problem).MaximumLength(5000);
        RuleFor(x => x.Customer).MaximumLength(5000);
        RuleFor(x => x.ValueProposition).MaximumLength(5000);
        RuleFor(x => x.Solution).MaximumLength(5000);
        RuleFor(x => x.BusinessModel).MaximumLength(5000);
        RuleFor(x => x.Market).MaximumLength(5000);
        RuleFor(x => x.Competitors).MaximumLength(5000);
        RuleFor(x => x.Pricing).MaximumLength(5000);
        RuleFor(x => x.Marketing).MaximumLength(5000);
        RuleFor(x => x.Sales).MaximumLength(5000);
        RuleFor(x => x.Operations).MaximumLength(5000);
        RuleFor(x => x.Technology).MaximumLength(5000);
        RuleFor(x => x.FinancialAssumptions).MaximumLength(5000);
        RuleFor(x => x.Risks).MaximumLength(5000);
    }
}

public sealed class UpdateBusinessIdeaCommandHandler(
    IBusinessIdeasRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<UpdateBusinessIdeaCommand, BusinessIdeaDto>
{
    public async Task<BusinessIdeaDto> Handle(UpdateBusinessIdeaCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        if (!dream.IsBusinessShaped)
        {
            throw new ConflictException("Mark this dream as business-shaped before building a business profile.");
        }

        var idea = await repository.GetForDreamAsync(dream.DreamId, cancellationToken);
        var isNew = idea is null;
        idea ??= BusinessIdea.Create(dream.DreamId, userId);

        idea.Problem = request.Problem;
        idea.Customer = request.Customer;
        idea.ValueProposition = request.ValueProposition;
        idea.Solution = request.Solution;
        idea.BusinessModel = request.BusinessModel;
        idea.Market = request.Market;
        idea.Competitors = request.Competitors;
        idea.Pricing = request.Pricing;
        idea.Marketing = request.Marketing;
        idea.Sales = request.Sales;
        idea.Operations = request.Operations;
        idea.Technology = request.Technology;
        idea.FinancialAssumptions = request.FinancialAssumptions;
        idea.Risks = request.Risks;
        idea.UpdatedBy = userId;

        if (isNew)
        {
            await repository.AddAsync(idea, cancellationToken);
        }
        else
        {
            await repository.SaveAsync(idea, cancellationToken);
        }

        return BusinessIdeaDto.From(idea);
    }
}
