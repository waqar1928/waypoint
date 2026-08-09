using Waypoint.BusinessIdeas.Domain;

namespace Waypoint.BusinessIdeas.Application;

public sealed record ViabilityEstimateDraft(
    int? ViabilityEstimate,
    List<string> StrongAssumptions,
    List<string> WeakAssumptions,
    List<string> Unknowns,
    List<string> RecommendedExperiments);

/// <summary>
/// Swappable heuristic-now/AI-later port — same pattern as IDreamDirectionGenerator (Dreams
/// module, Phase 2) and IPlanDraftGenerator (Goals module, Phase 3). Today's implementation is a
/// deterministic, non-AI heuristic; a real model can implement this interface later without
/// touching the command/handler that calls it.
/// </summary>
public interface IViabilityEstimateGenerator
{
    ViabilityEstimateDraft Generate(BusinessIdea idea, string dreamTitle);
}
