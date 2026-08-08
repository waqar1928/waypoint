using Microsoft.EntityFrameworkCore;
using Waypoint.Goals.Application;
using Waypoint.Goals.Domain;

namespace Waypoint.Goals.Infrastructure;

public sealed class GoalsRepository(GoalsDbContext db) : IGoalsRepository
{
    public Task<bool> HasPlanAsync(Guid dreamId, CancellationToken cancellationToken) =>
        db.Goals.AnyAsync(g => g.DreamId == dreamId && g.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<Goal>> GetGoalsForDreamAsync(Guid dreamId, CancellationToken cancellationToken) =>
        await db.Goals.Where(g => g.DreamId == dreamId && g.DeletedAt == null)
            .OrderBy(g => g.Horizon)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Mission>> GetMissionsForDreamAsync(Guid dreamId, CancellationToken cancellationToken)
    {
        var goalIds = await db.Goals.Where(g => g.DreamId == dreamId && g.DeletedAt == null)
            .Select(g => g.Id)
            .ToListAsync(cancellationToken);

        return await db.Missions
            .Where(m => goalIds.Contains(m.GoalId) && m.DeletedAt == null)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Goal?> GetGoalByIdAsync(Guid goalId, CancellationToken cancellationToken) =>
        db.Goals.SingleOrDefaultAsync(g => g.Id == goalId && g.DeletedAt == null, cancellationToken);

    public Task<Mission?> GetMissionByIdAsync(Guid missionId, CancellationToken cancellationToken) =>
        db.Missions.SingleOrDefaultAsync(m => m.Id == missionId && m.DeletedAt == null, cancellationToken);

    public async Task SaveGoalsAndMissionAsync(IReadOnlyList<Goal> goals, Mission mission, CancellationToken cancellationToken)
    {
        db.Goals.AddRange(goals);
        db.Missions.Add(mission);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveGoalAsync(Goal goal, CancellationToken cancellationToken)
    {
        if (db.Entry(goal).State == EntityState.Detached)
        {
            db.Goals.Update(goal);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveMissionAsync(Mission mission, CancellationToken cancellationToken)
    {
        if (db.Entry(mission).State == EntityState.Detached)
        {
            db.Missions.Update(mission);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Milestone>> GetMilestonesForDreamAsync(Guid dreamId, CancellationToken cancellationToken) =>
        await db.Milestones.Where(m => m.DreamId == dreamId)
            .OrderByDescending(m => m.AchievedAt ?? DateTimeOffset.MaxValue)
            .ThenByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddMilestoneAsync(Milestone milestone, CancellationToken cancellationToken)
    {
        db.Milestones.Add(milestone);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveMilestoneAsync(Milestone milestone, CancellationToken cancellationToken)
    {
        if (db.Entry(milestone).State == EntityState.Detached)
        {
            db.Milestones.Update(milestone);
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}
