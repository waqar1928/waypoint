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
    Guid? MissionId,
    /// <summary>
    /// A one-line, plain-language explanation of why this action is the recommended next move.
    /// Only ever populated by GetNextBestActionQuery/SetNextBestActionCommand - null everywhere
    /// else (a plain list of actions has no single "next" to explain). See
    /// NextBestActionSelector for how this gets computed.
    /// </summary>
    string? Rationale = null)
{
    public static ActionDto From(ActionItem a, string? rationale = null) => new(
        a.Id, a.Title, a.Description, a.Priority, a.Difficulty, a.EstimatedMinutes,
        a.ExpectedImpact, a.DueDate, a.Status, a.IsNextBestAction, a.GoalId, a.MissionId, rationale);
}
