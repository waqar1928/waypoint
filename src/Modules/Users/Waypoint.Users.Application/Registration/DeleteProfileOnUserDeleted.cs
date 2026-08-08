using MediatR;
using Waypoint.Common;

namespace Waypoint.Users.Application.Registration;

public sealed class DeleteProfileOnUserDeleted(IUsersRepository repository)
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    public Task Handle(UserDeletedIntegrationEvent notification, CancellationToken cancellationToken) =>
        repository.DeleteAllForUserAsync(notification.UserId, cancellationToken);
}
