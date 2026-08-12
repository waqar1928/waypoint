using NSubstitute;
using Waypoint.Common;
using Waypoint.Notifications.Application;
using Waypoint.Notifications.Application.Registration;

namespace Waypoint.Notifications.Tests;

public class DeleteNotificationsOnUserDeletedTests
{
    [Fact]
    public async Task Deletes_all_notifications_for_the_user_when_the_account_is_deleted()
    {
        var repository = Substitute.For<INotificationRepository>();
        var userId = Guid.NewGuid();
        var handler = new DeleteNotificationsOnUserDeleted(repository);

        await handler.Handle(new UserDeletedIntegrationEvent(userId, null), CancellationToken.None);

        await repository.Received(1).DeleteAllForUserAsync(userId, Arg.Any<CancellationToken>());
    }
}
