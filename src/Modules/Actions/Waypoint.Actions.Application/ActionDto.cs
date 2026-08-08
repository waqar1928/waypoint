using Waypoint.Actions.Domain;

namespace Waypoint.Actions.Application;

public sealed record ActionDto(
    Guid Id,
    string Title,
    string? Description,
    ActionPriority Priority,
    ActionDifficulty Difficulty,
    int? EstimatedMinutes,
    ActionImpact ExpectedImpact,
    DateOnly? DueDate,
    ActionStatus Status,
    bool IsNextBestAction,
    Guid? GoalId,
    Guid? MissionId)
{
    public static ActionDto From(ActionItem a) => new(
        a.Id, a.Title, a.Description, a.Priority, a.Difficulty, a.EstimatedMinutes,
        a.ExpectedImpact, a.DueDate, a.Status, a.IsNextBestAction, a.GoalId, a.MissionId);
}
