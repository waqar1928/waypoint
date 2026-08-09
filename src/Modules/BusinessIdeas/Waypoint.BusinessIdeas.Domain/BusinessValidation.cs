using Waypoint.Common;

namespace Waypoint.BusinessIdeas.Domain;

/// <summary>
/// A single run of the Dream Viability Estimate against a BusinessIdea's current fields. This is
/// append-only history, not an editable record — re-running the estimate after updating the
/// business profile produces a new row rather than overwriting the last one (see
/// docs/04-database-design.md: business_idea_id is not unique, so a BusinessIdea can accumulate
/// many BusinessValidation rows over time).
/// </summary>
public sealed class BusinessValidation : Entity
{
    public Guid BusinessIdeaId { get; init; }

    /// <summary>0-100. Framed everywhere in the UI as a decision-support estimate, never a
    /// guarantee — see the mandatory disclaimer required by docs/01-product-requirements.md.</summary>
    public int? ViabilityEstimate { get; set; }

    public List<string> StrongAssumptions { get; set; } = [];
    public List<string> WeakAssumptions { get; set; } = [];
    public List<string> Unknowns { get; set; } = [];
    public List<string> RecommendedExperiments { get; set; } = [];

    /// <summary>True today (the heuristic generator stands in for a real model, same swappable
    /// pattern as Dream Directions and the Plan draft) — kept as a real field so a future
    /// AI-backed generator and a hypothetical manual/user-authored entry path are both honestly
    /// represented rather than assumed.</summary>
    public bool GeneratedByAi { get; set; } = true;

    public static BusinessValidation Create(
        Guid businessIdeaId, Guid userId, int? viabilityEstimate,
        List<string> strongAssumptions, List<string> weakAssumptions,
        List<string> unknowns, List<string> recommendedExperiments, bool generatedByAi) =>
        new()
        {
            BusinessIdeaId = businessIdeaId,
            ViabilityEstimate = viabilityEstimate,
            StrongAssumptions = strongAssumptions,
            WeakAssumptions = weakAssumptions,
            Unknowns = unknowns,
            RecommendedExperiments = recommendedExperiments,
            GeneratedByAi = generatedByAi,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
