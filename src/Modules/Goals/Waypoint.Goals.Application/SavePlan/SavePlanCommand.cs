using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Goals.Domain;

namespace Waypoint.Goals.Application.SavePlan;

public sealed record SavePlanCommand(
    string FiveYearVision, string ThreeYearDirection, string OneYearGoal, string NinetyDayMission)
    : IRequest<PlanDto>;

public sealed class SavePlanCommandValidator : AbstractValidator<SavePlanCommand>
{
    public SavePlanCommandValidator()
    {
        RuleFor(x => x.FiveYearVision).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.ThreeYearDirection).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.OneYearGoal).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.NinetyDayMission).NotEmpty().MaximumLength(1000);
    }
}

/// <summary>Creates the initial Goal cascade + first Mission for the user's dream. One plan per dream in Phase 3.</summary>
public sealed class SavePlanCommandHandler(
    IGoalsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<SavePlanCommand, PlanDto>
{
    public async Task<PlanDto> Handle(SavePlanCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        if (await repository.HasPlanAsync(dream.DreamId, cancellationToken))
        {
            throw new ConflictException("You already have a plan for this dream.");
        }

        var fiveYear = Goal.Create(dream.DreamId, userId, GoalHorizon.FiveYear, request.FiveYearVision, null);
        var threeYear = Goal.Create(dream.DreamId, userId, GoalHorizon.ThreeYear, request.ThreeYearDirection, null);
        var oneYear = Goal.Create(dream.DreamId, userId, GoalHorizon.OneYear, request.OneYearGoal, null);
        var mission = Mission.Create(oneYear.Id, userId, request.NinetyDayMission, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(90)));

        await repository.SaveGoalsAndMissionAsync([fiveYear, threeYear, oneYear], mission, cancellationToken);

        return new PlanDto(
            [ToDto(fiveYear), ToDto(threeYear), ToDto(oneYear)],
            [new MissionDto(mission.Id, mission.GoalId, mission.Title, mission.TargetDate)]);
    }

    internal static GoalDto ToDto(Goal goal) => new(goal.Id, goal.Horizon, goal.Statement, goal.TargetDate);
}
