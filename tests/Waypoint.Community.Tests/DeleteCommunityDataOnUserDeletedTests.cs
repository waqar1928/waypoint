using NSubstitute;
using Waypoint.Common;
using Waypoint.Community.Application;
using Waypoint.Community.Application.Registration;

namespace Waypoint.Community.Tests;

public class DeleteCommunityDataOnUserDeletedTests
{
    [Fact]
    public async Task Deletes_all_community_data_for_the_user_when_the_account_is_deleted()
    {
        var repository = Substitute.For<ICommunityRepository>();
        var userId = Guid.NewGuid();
        var handler = new DeleteCommunityDataOnUserDeleted(repository);

        await handler.Handle(new UserDeletedIntegrationEvent(userId, null), CancellationToken.None);

        await repository.Received(1).DeleteAllForUserAsync(userId, Arg.Any<CancellationToken>());
    }
}
