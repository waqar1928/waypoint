using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Experiments.Domain;

namespace Waypoint.Experiments.Application.RecordExperimentResult;

public sealed record RecordExperimentResultCommand(
    Guid ExperimentId, ExperimentOutcome Outcome, string? Evidence, string Learning) : IRequest<ExperimentDto>;

public sealed class RecordExperimentResultCommandValidator : AbstractValidator<RecordExperimentResultCommand>
{
    public RecordExperimentResultCommandValidator()
    {
        RuleFor(x => x.ExperimentId).NotEmpty();
        RuleFor(x => x.Outcome).IsInEnum();
        RuleFor(x => x.Evidence).MaximumLength(2000);
        RuleFor(x => x.Learning).NotEmpty().MaximumLength(2000);
    }
}

public sealed class RecordExperimentResultCommandHandler(
    IExperimentsRepository repository, IDreamSummaryProvider dreamSummaryProvider,
    ICurrentUserAccessor currentUser, IAuditSink auditSink)
    : IRequestHandler<RecordExperimentResultCommand, ExperimentDto>
{
    public async Task<ExperimentDto> Handle(RecordExperimentResultCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        var experiment = await repository.GetByIdAsync(request.ExperimentId, cancellationToken);
        if (experiment is null || experiment.DreamId != dream.DreamId)
        {
            throw new NotFoundException("Experiment not found.");
        }

        var result = ExperimentResult.Create(experiment.Id, userId, request.Outcome, request.Evidence, request.Learning);
        await repository.AddResultAsync(result, cancellationToken);

        // Recording a result is how an experiment concludes — it always finalizes status to
        // Completed, regardless of whether the user had already flipped it from Planned to Running.
        experiment.Status = ExperimentStatus.Completed;
        experiment.UpdatedBy = userId;
        await repository.SaveAsync(experiment, cancellationToken);

        await auditSink.RecordAsync(
            new AuditEntry("Experiment", experiment.Id, "ResultRecorded", userId, null, DateTimeOffset.UtcNow),
            cancellationToken);

        var results = await repository.GetResultsForExperimentsAsync([experiment.Id], cancellationToken);
        return ExperimentDto.From(experiment, results);
    }
}
