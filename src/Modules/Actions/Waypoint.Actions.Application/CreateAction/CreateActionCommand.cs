using FluentValidation;
using MediatR;
using Waypoint.Actions.Domain;
using Waypoint.Common;

namespace Waypoint.Actions.Application.CreateAction;

public sealed record CreateActionCommand(
    string Title,
    string? Description,
    ActionPriority Priority,
    ActionDifficulty Difficulty,
    int? EstimatedMinutes,
    ActionImpact ExpectedImpact,
    DateOnly? DueDate,
    Guid? GoalId,
    Guid? MissionId) : IRequest<ActionDto>;

public sealed class CreateActionCommandValidator : AbstractValidator<CreateActionCommand>
{
    public CreateActionCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Priority).IsInEnum();
        RuleFor(x => x.Difficulty).IsInEnum();
        RuleFor(x => x.ExpectedImpact).IsInEnum();
        RuleFor(x => x.EstimatedMinutes).GreaterThan(0).When(x => x.EstimatedMinutes.HasValue);
    }
}

public sealed class CreateActionCommandHandler(
    IActionsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<CreateActionCommand, ActionDto>
{
    public async Task<ActionDto> Handle(CreateActionCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        var action = ActionItem.Create(
            dream.DreamId, userId, request.Title, request.Description, request.Priority,
            request.Difficulty, request.EstimatedMinutes, request.ExpectedImpact, request.DueDate,
            request.GoalId, request.MissionId);

        await repository.AddAsync(action, cancellationToken);

        return ActionDto.From(action);
    }
}
