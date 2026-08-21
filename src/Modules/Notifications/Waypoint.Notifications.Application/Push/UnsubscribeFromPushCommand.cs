using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Application.Push;

public sealed record UnsubscribeFromPushCommand(Guid SubscriptionId) : IRequest;

public sealed class UnsubscribeFromPushCommandValidator : AbstractValidator<UnsubscribeFromPushCommand>
{
    public UnsubscribeFromPushCommandValidator()
    {
        RuleFor(x => x.SubscriptionId).NotEmpty();
    }
}

public sealed class UnsubscribeFromPushCommandHandler(IPushSubscriptionRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<UnsubscribeFromPushCommand>
{
    public async Task Handle(UnsubscribeFromPushCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var subscription = await repository.GetByIdAsync(request.SubscriptionId, cancellationToken);

        // NotFoundException (never Forbidden) for a mismatched owner - same anti-enumeration
        // reasoning used everywhere else in this codebase (see MarkNotificationReadCommandHandler):
        // a 403 would confirm the subscription exists and just isn't yours, a 404 doesn't.
        if (subscription is null || subscription.UserId != userId)
        {
            throw new NotFoundException("Push subscription not found.");
        }

        if (subscription.Status == PushSubscriptionStatus.Active)
        {
            await repository.DeactivateAsync(subscription, "UserUnsubscribed", cancellationToken);
        }
    }
}
