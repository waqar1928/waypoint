using FluentValidation;
using MediatR;
using Waypoint.Actions.Domain;
using Waypoint.Common;

namespace Waypoint.Actions.Application.SetNextBestAction;

public sealed record SetNextBestActionCommand(Guid ActionId) : IRequest<ActionDto>;

public sealed class SetNextBestActionCommandValidator : AbstractValidator<SetNextBestActionCommand>
{
    public SetNextBestActionCommandValidator()
    {
        RuleFor(x => x.ActionId).NotEmpty();
    }
}

/// <summary>
/// Only one action can be "next" per dream at a time (also enforced by a
/// unique filtered index — see docs/04-database-design.md). Clearing every
/// other flag first, then setting this one, keeps that invariant even if
/// two requests race.
/// </summary>
public sealed class SetNextBestActionCommandHandler(
    IActionsRepository repository, IDreamSummaryProvider dreamSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<SetNextBestActionCommand, ActionDto>
{
    public async Task<ActionDto> Handle(SetNextBestActionCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        var action = await repository.GetByIdAsync(request.ActionId, cancellationToken);
        if (action is null || action.DreamId != dream.DreamId)
        {
            throw new NotFoundException("Action not found.");
        }

        if (action.Status is ActionStatus.Completed or ActionStatus.Cancelled)
        {
            throw new ConflictException("A completed or cancelled action can't be the next best action.");
        }

        await repository.ClearNextBestActionForDreamAsync(dream.DreamId, action.Id, cancellationToken);

        action.IsNextBestAction = true;
        action.UpdatedBy = userId;
        await repository.SaveAsync(action, cancellationToken);

        return ActionDto.From(action);
    }
}
