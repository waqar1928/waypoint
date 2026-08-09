using Waypoint.Common;

namespace Waypoint.Experiments.Domain;

/// <summary>
/// What actually happened when an Experiment was run. Recording a result is what closes the
/// learning loop back into Journal/Momentum (see docs/02-user-journey.md, "Learn & Grow").
///
/// Phase 4 scope: the docs sketch an optional next_experiment_id self-link chaining a result to a
/// follow-up experiment; that field is deferred (no UI flow for it exists yet) — same call as
/// deferring Project in Phase 3. Users start a new Experiment the normal way after learning from
/// this one.
/// </summary>
public sealed class ExperimentResult : Entity
{
    public Guid ExperimentId { get; init; }
    public ExperimentOutcome Outcome { get; set; }
    public string? Evidence { get; set; }
    public string Learning { get; set; } = string.Empty;

    public static ExperimentResult Create(
        Guid experimentId, Guid userId, ExperimentOutcome outcome, string? evidence, string learning) =>
        new()
        {
            ExperimentId = experimentId,
            Outcome = outcome,
            Evidence = evidence,
            Learning = learning,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
