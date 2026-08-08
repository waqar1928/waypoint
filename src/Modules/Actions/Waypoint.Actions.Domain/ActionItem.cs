using Waypoint.Common;

namespace Waypoint.Actions.Domain;

/// <summary>
/// Named ActionItem (not Action) to avoid colliding with System.Action —
/// every consuming project has ImplicitUsings enabled, which pulls in
/// System, so an unqualified "Action" would be ambiguous outside this
/// project. The product/API vocabulary is still "action" throughout.
/// </summary>
public sealed class ActionItem : Entity, ISoftDeletable
{
    public Guid DreamId { get; init; }
    public Guid? GoalId { get; set; }
    public Guid? MissionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActionPriority Priority { get; set; } = ActionPriority.Medium;
    public ActionDifficulty Difficulty { get; set; } = ActionDifficulty.Medium;
    public int? EstimatedMinutes { get; set; }
    public ActionImpact ExpectedImpact { get; set; } = ActionImpact.Medium;
    public DateOnly? DueDate { get; set; }
    public ActionStatus Status { get; set; } = ActionStatus.NotStarted;
    public bool IsNextBestAction { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public static ActionItem Create(
        Guid dreamId, Guid userId, string title, string? description,
        ActionPriority priority, ActionDifficulty difficulty, int? estimatedMinutes,
        ActionImpact expectedImpact, DateOnly? dueDate, Guid? goalId, Guid? missionId) =>
        new()
        {
            DreamId = dreamId,
            GoalId = goalId,
            MissionId = missionId,
            Title = title,
            Description = description,
            Priority = priority,
            Difficulty = difficulty,
            EstimatedMinutes = estimatedMinutes,
            ExpectedImpact = expectedImpact,
            DueDate = dueDate,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
