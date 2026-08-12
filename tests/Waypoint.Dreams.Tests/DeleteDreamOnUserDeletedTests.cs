using NSubstitute;
using Waypoint.Common;
using Waypoint.Dreams.Application;
using Waypoint.Dreams.Application.Registration;

namespace Waypoint.Dreams.Tests;

public class DeleteDreamOnUserDeletedTests
{
    [Fact]
    public async Task Deletes_the_users_dream_when_the_account_is_deleted()
    {
        var repository = Substitute.For<IDreamRepository>();
        var userId = Guid.NewGuid();
        var handler = new DeleteDreamOnUserDeleted(repository);

        await handler.Handle(new UserDeletedIntegrationEvent(userId, Guid.NewGuid()), CancellationToken.None);

        await repository.Received(1).DeleteForUserAsync(userId, Arg.Any<CancellationToken>());
    }
}
