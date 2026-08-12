using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Goals.Domain;

namespace Waypoint.Goals.Application.Milestones;

public sealed record GetMyMilestonesQuery : IRequest<IReadOnlyList<MilestoneDto>>;

public sealed record CreateMilestoneCommand(string Title) : IRequest<MilestoneDto>;

public sealed record MarkMilestoneAchievedCommand(Guid MilestoneId) : IRequest<MilestoneDto>;

public sealed class CreateMilestoneCommandValidator : AbstractValidator<CreateMilestoneCommand>
{
    public CreateMilestoneCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}

public sealed class GetMyMilestonesQueryHandler(
    IGoalsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyMilestonesQuery, IReadOnlyList<MilestoneDto>>
{
    public async Task<IReadOnlyList<MilestoneDto>> Handle(GetMyMilestonesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken);
        if (dream is null)
        {
            return [];
        }

        var milestones = await repository.GetMilestonesForDreamAsync(dream.DreamId, cancellationToken);
        return milestones.Select(ToDto).ToList();
    }

    internal static MilestoneDto ToDto(Milestone m) => new(m.Id, m.Title, m.AchievedAt, m.IsCustom);
}

public sealed class CreateMilestoneCommandHandler(
    IGoalsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<CreateMilestoneCommand, MilestoneDto>
{
    public async Task<MilestoneDto> Handle(CreateMilestoneCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        var milestone = Milestone.Create(dream.DreamId, userId, request.Title, isCustom: true);
        await repository.AddMilestoneAsync(milestone, cancellationToken);

        return GetMyMilestonesQueryHandler.ToDto(milestone);
    }
}

public sealed class MarkMilestoneAchievedCommandHandler(
    IGoalsRepository repository, IDreamSummaryProvider dreamSummaryProvider,
    ICurrentUserAccessor currentUser, IProductAnalyticsSink analyticsSink)
    : IRequestHandler<MarkMilestoneAchievedCommand, MilestoneDto>
{
    public async Task<MilestoneDto> Handle(MarkMilestoneAchievedCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        var milestone = await repository.GetMilestoneByIdAsync(request.MilestoneId, cancellationToken);
        if (milestone is null || milestone.DreamId != dream.DreamId)
        {
            throw new NotFoundException("Milestone not found.");
        }

        milestone.AchievedAt = DateTimeOffset.UtcNow;
        milestone.UpdatedBy = userId;

        await repository.SaveMilestoneAsync(milestone, cancellationToken);

        await analyticsSink.TrackAsync(
            new AnalyticsEvent(AnalyticsEvents.MilestoneAchieved, userId, null, DateTimeOffset.UtcNow), cancellationToken);

        return GetMyMilestonesQueryHandler.ToDto(milestone);
    }
}
