using FluentValidation;
using MediatR;
using Waypoint.Common;

namespace Waypoint.Goals.Application.UpdateMission;

public sealed record UpdateMissionCommand(Guid MissionId, string Title, DateOnly? TargetDate) : IRequest<MissionDto>;

public sealed class UpdateMissionCommandValidator : AbstractValidator<UpdateMissionCommand>
{
    public UpdateMissionCommandValidator()
    {
        RuleFor(x => x.MissionId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

public sealed class UpdateMissionCommandHandler(
    IGoalsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<UpdateMissionCommand, MissionDto>
{
    public async Task<MissionDto> Handle(UpdateMissionCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        var mission = await repository.GetMissionByIdAsync(request.MissionId, cancellationToken)
            ?? throw new NotFoundException("Mission not found.");

        var goal = await repository.GetGoalByIdAsync(mission.GoalId, cancellationToken);
        if (goal is null || goal.DreamId != dream.DreamId)
        {
            throw new NotFoundException("Mission not found.");
        }

        mission.Title = request.Title;
        mission.TargetDate = request.TargetDate;
        mission.UpdatedBy = userId;

        await repository.SaveMissionAsync(mission, cancellationToken);

        return new MissionDto(mission.Id, mission.GoalId, mission.Title, mission.TargetDate);
    }
}
