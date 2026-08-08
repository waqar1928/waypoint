using Waypoint.Common;

namespace Waypoint.Goals.Domain;

public sealed class Milestone : Entity
{
    public Guid DreamId { get; init; }
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset? AchievedAt { get; set; }
    public bool IsCustom { get; set; }

    public static Milestone Create(Guid dreamId, Guid userId, string title, bool isCustom) =>
        new()
        {
            DreamId = dreamId,
            Title = title,
            IsCustom = isCustom,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
