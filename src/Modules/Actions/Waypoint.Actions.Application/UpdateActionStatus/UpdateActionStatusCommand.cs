using FluentValidation;
using MediatR;
using Waypoint.Actions.Domain;
using Waypoint.Common;

namespace Waypoint.Actions.Application.UpdateActionStatus;

public sealed record UpdateActionStatusCommand(Guid ActionId, ActionStatus Status) : IRequest<ActionDto>;

public sealed class UpdateActionStatusCommandValidator : AbstractValidator<UpdateActionStatusCommand>
{
    public UpdateActionStatusCommandValidator()
    {
        RuleFor(x => x.ActionId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}

public sealed class UpdateActionStatusCommandHandler(
    IActionsRepository repository, IDreamSummaryProvider dreamSummaryProvider,
    ICurrentUserAccessor currentUser, IAuditSink auditSink)
    : IRequestHandler<UpdateActionStatusCommand, ActionDto>
{
    public async Task<ActionDto> Handle(UpdateActionStatusCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        var action = await repository.GetByIdAsync(request.ActionId, cancellationToken);
        if (action is null || action.DreamId != dream.DreamId)
        {
            throw new NotFoundException("Action not found.");
        }

        action.Status = request.Status;
        action.CompletedAt = request.Status == ActionStatus.Completed ? DateTimeOffset.UtcNow : null;
        action.UpdatedBy = userId;

        // A completed/cancelled action can't stay "next" — clear the flag so the dashboard never
        // points at something that's already done.
        if (request.Status is ActionStatus.Completed or ActionStatus.Cancelled)
        {
            action.IsNextBestAction = false;
        }

        await repository.SaveAsync(action, cancellationToken);

        if (request.Status == ActionStatus.Completed)
        {
            await auditSink.RecordAsync(
                new AuditEntry("Action", action.Id, "Completed", userId, null, DateTimeOffset.UtcNow), cancellationToken);
        }

        return ActionDto.From(action);
    }
}
