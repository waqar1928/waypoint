using Waypoint.Experiments.Domain;

namespace Waypoint.Experiments.Application;

public sealed record ExperimentResultDto(Guid Id, ExperimentOutcome Outcome, string? Evidence, string Learning, DateTimeOffset CreatedAt)
{
    public static ExperimentResultDto From(ExperimentResult r) => new(r.Id, r.Outcome, r.Evidence, r.Learning, r.CreatedAt);
}

public sealed record ExperimentDto(
    Guid Id,
    string IdeaDescription,
    string Hypothesis,
    string SuccessCriteria,
    ExperimentStatus Status,
    IReadOnlyList<ExperimentResultDto> Results)
{
    public static ExperimentDto From(Experiment e, IReadOnlyList<ExperimentResult> results) => new(
        e.Id, e.IdeaDescription, e.Hypothesis, e.SuccessCriteria, e.Status,
        results.Select(ExperimentResultDto.From).ToList());
}
