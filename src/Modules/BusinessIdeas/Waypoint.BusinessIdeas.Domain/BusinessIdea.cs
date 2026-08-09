using Waypoint.Common;

namespace Waypoint.BusinessIdeas.Domain;

/// <summary>
/// The working profile of a business-shaped Dream — one per Dream (see docs/03-domain-model.md,
/// Dream 1───0..1 BusinessIdea). Every field is optional and free text; the user fills in what
/// they know and leaves the rest blank, then can generate a BusinessValidation against whatever
/// is filled in so far.
/// </summary>
public sealed class BusinessIdea : Entity
{
    public Guid DreamId { get; init; }
    public string? Problem { get; set; }
    public string? Customer { get; set; }
    public string? ValueProposition { get; set; }
    public string? Solution { get; set; }
    public string? BusinessModel { get; set; }
    public string? Market { get; set; }
    public string? Competitors { get; set; }
    public string? Pricing { get; set; }
    public string? Marketing { get; set; }
    public string? Sales { get; set; }
    public string? Operations { get; set; }
    public string? Technology { get; set; }
    public string? FinancialAssumptions { get; set; }
    public string? Risks { get; set; }

    public static BusinessIdea Create(Guid dreamId, Guid userId) =>
        new()
        {
            DreamId = dreamId,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
