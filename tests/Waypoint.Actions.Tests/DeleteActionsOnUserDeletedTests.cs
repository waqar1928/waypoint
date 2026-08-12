using NSubstitute;
using Waypoint.Actions.Application;
using Waypoint.Actions.Application.Registration;
using Waypoint.Common;

namespace Waypoint.Actions.Tests;

public class DeleteActionsOnUserDeletedTests
{
    [Fact]
    public async Task Deletes_actions_for_the_users_dream_when_the_account_is_deleted()
    {
        var repository = Substitute.For<IActionsRepository>();
        var userId = Guid.NewGuid();
        var dreamId = Guid.NewGuid();
        var handler = new DeleteActionsOnUserDeleted(repository);

        await handler.Handle(new UserDeletedIntegrationEvent(userId, dreamId), CancellationToken.None);

        await repository.Received(1).DeleteAllForDreamAsync(dreamId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_nothing_when_the_user_never_had_a_dream()
    {
        var repository = Substitute.For<IActionsRepository>();
        var handler = new DeleteActionsOnUserDeleted(repository);

        await handler.Handle(new UserDeletedIntegrationEvent(Guid.NewGuid(), null), CancellationToken.None);

        await repository.DidNotReceive().DeleteAllForDreamAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
