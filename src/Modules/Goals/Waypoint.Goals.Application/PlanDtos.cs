using Waypoint.Goals.Domain;

namespace Waypoint.Goals.Application;

public sealed record GoalDto(Guid Id, GoalHorizon Horizon, string Statement, DateOnly? TargetDate);

public sealed record MissionDto(Guid Id, Guid GoalId, string Title, DateOnly? TargetDate);

public sealed record MilestoneDto(Guid Id, string Title, DateTimeOffset? AchievedAt, bool IsCustom);

public sealed record PlanDto(IReadOnlyList<GoalDto> Goals, IReadOnlyList<MissionDto> Missions);

/// <summary>Draft plan text, not yet persisted — the user edits and confirms before SavePlanCommand runs.</summary>
public sealed record PlanDraftDto(string FiveYearVision, string ThreeYearDirection, string OneYearGoal, string NinetyDayMission);
