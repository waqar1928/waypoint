using FluentValidation;
using MediatR;
using Waypoint.Common;

namespace Waypoint.Goals.Application.UpdateGoal;

public sealed record UpdateGoalCommand(Guid GoalId, string Statement, DateOnly? TargetDate) : IRequest<GoalDto>;

public sealed class UpdateGoalCommandValidator : AbstractValidator<UpdateGoalCommand>
{
    public UpdateGoalCommandValidator()
    {
        RuleFor(x => x.GoalId).NotEmpty();
        RuleFor(x => x.Statement).NotEmpty().MaximumLength(1000);
    }
}

public sealed class UpdateGoalCommandHandler(
    IGoalsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<UpdateGoalCommand, GoalDto>
{
    public async Task<GoalDto> Handle(UpdateGoalCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        var goal = await repository.GetGoalByIdAsync(request.GoalId, cancellationToken);
        if (goal is null || goal.DreamId != dream.DreamId)
        {
            throw new NotFoundException("Goal not found.");
        }

        goal.Statement = request.Statement;
        goal.TargetDate = request.TargetDate;
        goal.UpdatedBy = userId;

        await repository.SaveGoalAsync(goal, cancellationToken);

        return new GoalDto(goal.Id, goal.Horizon, goal.Statement, goal.TargetDate);
    }
}
