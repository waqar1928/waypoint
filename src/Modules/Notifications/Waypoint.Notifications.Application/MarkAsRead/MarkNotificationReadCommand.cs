using FluentValidation;
using MediatR;
using Waypoint.Common;

namespace Waypoint.Notifications.Application.MarkAsRead;

public sealed record MarkNotificationReadCommand(Guid NotificationId) : IRequest;

public sealed class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(x => x.NotificationId).NotEmpty();
    }
}

public sealed class MarkNotificationReadCommandHandler(INotificationRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var notification = await repository.GetByIdAsync(request.NotificationId, cancellationToken);

        // NotFoundException (never Forbidden) for a mismatched owner — same anti-enumeration
        // reasoning as every other ownership check in this codebase (see
        // docs/PRODUCTION_READINESS_AUDIT.md's Authorization section): a 403 would confirm the
        // notification exists and just isn't yours, a 404 doesn't.
        if (notification is null || notification.RecipientUserId != userId)
        {
            throw new NotFoundException("Notification not found.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTimeOffset.UtcNow;
            await repository.SaveAsync(notification, cancellationToken);
        }
    }
}
