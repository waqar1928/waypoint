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
    Task AddMilestoneAsync(Milestone milestone, CancellationToken cancellationToken);
    Task SaveMilestoneAsync(Milestone milestone, CancellationToken cancellationToken);
}
