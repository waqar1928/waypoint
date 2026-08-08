using Waypoint.Common;

namespace Waypoint.Goals.Domain;

public sealed class Mission : Entity, ISoftDeletable
{
    public Guid GoalId { get; init; }
    public string Title { get; set; } = string.Empty;
    public DateOnly? TargetDate { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public static Mission Create(Guid goalId, Guid userId, string title, DateOnly? targetDate) =>
        new()
        {
            GoalId = goalId,
            Title = title,
            TargetDate = targetDate,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
