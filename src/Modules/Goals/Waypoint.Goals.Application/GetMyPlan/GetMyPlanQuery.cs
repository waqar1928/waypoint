using MediatR;
using Waypoint.Common;

namespace Waypoint.Goals.Application.GetMyPlan;

public sealed record GetMyPlanQuery : IRequest<PlanDto?>;

public sealed class GetMyPlanQueryHandler(IGoalsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyPlanQuery, PlanDto?>
{
    public async Task<PlanDto?> Handle(GetMyPlanQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);
        if (dream is null)
        {
            return null;
        }

        var goals = await repository.GetGoalsForDreamAsync(dream.DreamId, cancellationToken);
        if (goals.Count == 0)
        {
            return null;
        }

        var missions = await repository.GetMissionsForDreamAsync(dream.DreamId, cancellationToken);

        return new PlanDto(
            goals.Select(g => new GoalDto(g.Id, g.Horizon, g.Statement, g.TargetDate)).ToList(),
            missions.Select(m => new MissionDto(m.Id, m.GoalId, m.Title, m.TargetDate)).ToList());
    }
}
