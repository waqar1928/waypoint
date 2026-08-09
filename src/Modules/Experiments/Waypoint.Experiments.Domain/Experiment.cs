using Waypoint.Common;

namespace Waypoint.Experiments.Domain;

/// <summary>
/// The smallest testable step toward a Dream — a cheap way to check an assumption before a big
/// commitment (see docs/01-product-requirements.md, "Validate" stage of the Waypoint Arc).
/// </summary>
public sealed class Experiment : Entity, ISoftDeletable
{
    public Guid DreamId { get; init; }
    public string IdeaDescription { get; set; } = string.Empty;
    public string Hypothesis { get; set; } = string.Empty;
    public string SuccessCriteria { get; set; } = string.Empty;
    public ExperimentStatus Status { get; set; } = ExperimentStatus.Planned;
    public DateTimeOffset? DeletedAt { get; set; }

    public static Experiment Create(
        Guid dreamId, Guid userId, string ideaDescription, string hypothesis, string successCriteria) =>
        new()
        {
            DreamId = dreamId,
            IdeaDescription = ideaDescription,
            Hypothesis = hypothesis,
            SuccessCriteria = successCriteria,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
