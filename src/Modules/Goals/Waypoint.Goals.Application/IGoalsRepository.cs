using Waypoint.Goals.Domain;

namespace Waypoint.Goals.Application;

public interface IGoalsRepository
{
    Task<bool> HasPlanAsync(Guid dreamId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Goal>> GetGoalsForDreamAsync(Guid dreamId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Mission>> GetMissionsForDreamAsync(Guid dreamId, CancellationToken cancellationToken);
    Task<Goal?> GetGoalByIdAsync(Guid goalId, CancellationToken cancellationToken);
    Task<Mission?> GetMissionByIdAsync(Guid missionId, CancellationToken cancellationToken);

    Task SaveGoalsAndMissionAsync(
        IReadOnlyList<Goal> goals, Mission mission, CancellationToken cancellationToken);

    Task SaveGoalAsync(Goal goal, CancellationToken cancellationToken);
    Task SaveMissionAsync(Mission mission, CancellationToken cancellationToken);

    Task<IReadOnlyList<Milestone>> GetMilestonesForDreamAsync(Guid dreamId, CancellationToken cancellationToken);
    Task<Milestone?> GetMilestoneByIdAsync(Guid milestoneId, CancellationToken cancellationToken);
    Task AddMilestoneAsync(Milestone milestone, CancellationToken cancellationToken);
    Task SaveMilestoneAsync(Milestone milestone, CancellationToken cancellationToken);

    /// <summary>Real hard delete of every Goal/Mission/Milestone for one dream — backs account
    /// deletion (see docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section). Not filtered
    /// on DeletedAt, so already-soft-deleted rows get actually purged too. Mission has no DreamId
    /// column of its own (only GoalId — see GetMissionsForDreamAsync's existing two-step
    /// resolution), so this needs the same goalIds-first approach to find missions to delete.</summary>
    Task DeleteAllForDreamAsync(Guid dreamId, CancellationToken cancellationToken);
}
