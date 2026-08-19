using FluentValidation;
using MediatR;
using Waypoint.Actions.Domain;
using Waypoint.Common;

namespace Waypoint.Actions.Application.AddActionReflection;

/// <summary>
/// Optional follow-up to UpdateActionStatusCommand, not part of it - completing an action never
/// blocks on this. If the user chooses to answer "what happened" / "what did you learn," this is
/// what captures it. Nothing here changes the Action itself (no new columns, no migration); when a
/// learning is given, it's published as LearningCapturedIntegrationEvent so it becomes a Journal
/// entry, same as an Experiment's result - see that event's doc comment for why.
/// </summary>
public sealed record AddActionReflectionCommand(Guid ActionId, string? WhatHappened, string? Learning) : IRequest;

public sealed class AddActionReflectionCommandValidator : AbstractValidator<AddActionReflectionCommand>
{
    public AddActionReflectionCommandValidator()
    {
        RuleFor(x => x.ActionId).NotEmpty();
        RuleFor(x => x.WhatHappened).MaximumLength(2000);
        RuleFor(x => x.Learning).MaximumLength(2000);
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.WhatHappened) || !string.IsNullOrWhiteSpace(x.Learning))
            .WithMessage("Say what happened or what you learned.");
    }
}

public sealed class AddActionReflectionCommandHandler(
    IActionsRepository repository, IDreamSummaryProvider dreamSummaryProvider,
    ICurrentUserAccessor currentUser, IPublisher publisher)
    : IRequestHandler<AddActionReflectionCommand>
{
    public async Task Handle(AddActionReflectionCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await dreamSummaryProvider.GetForUserAsync(userId, cancellationToken)
            ?? throw new NotFoundException("You don't have a dream yet.");

        var action = await repository.GetByIdAsync(request.ActionId, cancellationToken);
        if (action is null || action.DreamId != dream.DreamId)
        {
            throw new NotFoundException("Action not found.");
        }

        if (action.Status != ActionStatus.Completed)
        {
            throw new ConflictException("You can only reflect on a completed action.");
        }

        var body = (string.IsNullOrWhiteSpace(request.WhatHappened), string.IsNullOrWhiteSpace(request.Learning)) switch
        {
            (false, false) => $"{request.WhatHappened}\n\n{request.Learning}",
            (false, true) => request.WhatHappened!,
            (true, false) => request.Learning!,
            (true, true) => string.Empty, // unreachable — the validator requires at least one
        };

        await publisher.Publish(new LearningCapturedIntegrationEvent(userId, dream.DreamId, body), cancellationToken);
    }
}
