using Waypoint.Common;

namespace Waypoint.Goals.Domain;

public sealed class Goal : Entity, ISoftDeletable
{
    public Guid DreamId { get; init; }
    public GoalHorizon Horizon { get; set; }
    public string Statement { get; set; } = string.Empty;
    public DateOnly? TargetDate { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public static Goal Create(Guid dreamId, Guid userId, GoalHorizon horizon, string statement, DateOnly? targetDate) =>
        new()
        {
            DreamId = dreamId,
            Horizon = horizon,
            Statement = statement,
            TargetDate = targetDate,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
