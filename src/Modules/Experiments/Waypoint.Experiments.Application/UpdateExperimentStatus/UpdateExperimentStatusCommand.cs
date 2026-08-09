using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Experiments.Domain;

namespace Waypoint.Experiments.Application.UpdateExperimentStatus;

public sealed record UpdateExperimentStatusCommand(Guid ExperimentId, ExperimentStatus Status) : IRequest<ExperimentDto>;

public sealed class UpdateExperimentStatusCommandValidator : AbstractValidator<UpdateExperimentStatusCommand>
{
    public UpdateExperimentStatusCommandValidator()
    {
        RuleFor(x => x.ExperimentId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class UpdateExperimentStatusCommandHandler(
    IExperimentsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<UpdateExperimentStatusCommand, ExperimentDto>
{
    public async Task<ExperimentDto> Handle(UpdateExperimentStatusCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        var experiment = await repository.GetByIdAsync(request.ExperimentId, cancellationToken);
        if (experiment is null || experiment.DreamId != dream.DreamId)
        {
            throw new NotFoundException("Experiment not found.");
        }

        experiment.Status = request.Status;
        experiment.UpdatedBy = userId;
        await repository.SaveAsync(experiment, cancellationToken);

        var results = await repository.GetResultsForExperimentsAsync([experiment.Id], cancellationToken);
        return ExperimentDto.From(experiment, results);
    }
}
