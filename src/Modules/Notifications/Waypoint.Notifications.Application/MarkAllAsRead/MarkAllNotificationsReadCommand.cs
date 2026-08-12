using MediatR;
using Waypoint.Common;

namespace Waypoint.Notifications.Application.MarkAllAsRead;

public sealed record MarkAllNotificationsReadCommand : IRequest;

public sealed class MarkAllNotificationsReadCommandHandler(INotificationRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<MarkAllNotificationsReadCommand>
{
    public Task Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        return repository.MarkAllReadAsync(userId, cancellationToken);
    }
}
