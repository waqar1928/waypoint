using FluentValidation;
using MediatR;
using Waypoint.Actions.Domain;
using Waypoint.Common;

namespace Waypoint.Actions.Application.UpdateAction;

public sealed record UpdateActionCommand(
    Guid ActionId,
    string Title,
    string? Description,
    ActionPriority Priority,
    ActionDifficulty Difficulty,
    int? EstimatedMinutes,
    ActionImpact ExpectedImpact,
    DateOnly? DueDate) : IRequest<ActionDto>;

public sealed class UpdateActionCommandValidator : AbstractValidator<UpdateActionCommand>
{
    public UpdateActionCommandValidator()
    {
        RuleFor(x => x.ActionId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.EstimatedMinutes).GreaterThan(0).When(x => x.EstimatedMinutes.HasValue);
    }
}

public sealed class UpdateActionCommandHandler(
    IActionsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<UpdateActionCommand, ActionDto>
{
    public async Task<ActionDto> Handle(UpdateActionCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        var action = await repository.GetByIdAsync(request.ActionId, cancellationToken);
        if (action is null || action.DreamId != dream.DreamId)
        {
            throw new NotFoundException("Action not found.");
        }

        action.Title = request.Title;
        action.Description = request.Description;
        action.Priority = request.Priority;
        action.Difficulty = request.Difficulty;
        action.EstimatedMinutes = request.EstimatedMinutes;
        action.ExpectedImpact = request.ExpectedImpact;
        action.DueDate = request.DueDate;
        action.UpdatedBy = userId;

        await repository.SaveAsync(action, cancellationToken);

        return ActionDto.From(action);
    }
}
