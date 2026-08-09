using Waypoint.Common;

namespace Waypoint.Mentorship.Domain;

/// <summary>
/// Self-service mentor designation (see docs/01-product-requirements.md: "experienced people who
/// opt in to answer help requests"). VerificationStatus defaults to Unverified and stays that way
/// until Phase 8's admin verification workflow mutates it — an unverified mentor can still respond
/// to help requests today; verification is a trust signal, not a hard gate, per the PRD's opt-in
/// framing (no verification qualifier is mentioned there).
///
/// RatingAvg exists for schema forward-compatibility only — deferred in Phase 7 (no rating
/// submission workflow is built; see docs research: no criteria for when/how a rating happens).
/// </summary>
public sealed class MentorProfile : Entity
{
    public Guid UserId { get; init; }
    public List<string> Expertise { get; set; } = [];
    public int? YearsExperience { get; set; }
    public string? Availability { get; set; }
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Unverified;
    public decimal? RatingAvg { get; set; }

    public static MentorProfile Create(Guid userId, List<string> expertise, int? yearsExperience, string? availability) =>
        new()
        {
            UserId = userId,
            Expertise = expertise,
            YearsExperience = yearsExperience,
            Availability = availability,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
