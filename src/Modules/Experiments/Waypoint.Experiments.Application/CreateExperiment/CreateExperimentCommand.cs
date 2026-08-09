using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Experiments.Domain;

namespace Waypoint.Experiments.Application.CreateExperiment;

public sealed record CreateExperimentCommand(string IdeaDescription, string Hypothesis, string SuccessCriteria)
    : IRequest<ExperimentDto>;

public sealed class CreateExperimentCommandValidator : AbstractValidator<CreateExperimentCommand>
{
    public CreateExperimentCommandValidator()
    {
        RuleFor(x => x.IdeaDescription).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Hypothesis).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.SuccessCriteria).NotEmpty().MaximumLength(1000);
    }
}

public sealed class CreateExperimentCommandHandler(
    IExperimentsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<CreateExperimentCommand, ExperimentDto>
{
    public async Task<ExperimentDto> Handle(CreateExperimentCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        var experiment = Experiment.Create(
            dream.DreamId, userId, request.IdeaDescription, request.Hypothesis, request.SuccessCriteria);

        await repository.AddAsync(experiment, cancellationToken);

        return ExperimentDto.From(experiment, []);
    }
}
