using NSubstitute;
using Waypoint.AI.Application;
using Waypoint.AI.Application.Registration;
using Waypoint.Common;

namespace Waypoint.AI.Tests;

public class DeleteConversationsOnUserDeletedTests
{
    [Fact]
    public async Task Deletes_all_conversations_for_the_user_when_the_account_is_deleted()
    {
        var repository = Substitute.For<IAiRepository>();
        var userId = Guid.NewGuid();
        var handler = new DeleteConversationsOnUserDeleted(repository);

        await handler.Handle(new UserDeletedIntegrationEvent(userId, null), CancellationToken.None);

        await repository.Received(1).DeleteAllForUserAsync(userId, Arg.Any<CancellationToken>());
    }
}
