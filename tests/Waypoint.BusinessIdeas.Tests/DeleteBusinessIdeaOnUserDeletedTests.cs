using NSubstitute;
using Waypoint.BusinessIdeas.Application;
using Waypoint.BusinessIdeas.Application.Registration;
using Waypoint.Common;

namespace Waypoint.BusinessIdeas.Tests;

public class DeleteBusinessIdeaOnUserDeletedTests
{
    [Fact]
    public async Task Deletes_the_business_idea_for_the_users_dream_when_the_account_is_deleted()
    {
        var repository = Substitute.For<IBusinessIdeasRepository>();
        var userId = Guid.NewGuid();
        var dreamId = Guid.NewGuid();
        var handler = new DeleteBusinessIdeaOnUserDeleted(repository);

        await handler.Handle(new UserDeletedIntegrationEvent(userId, dreamId), CancellationToken.None);

        await repository.Received(1).DeleteForDreamAsync(dreamId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_nothing_when_the_user_never_had_a_dream()
    {
        var repository = Substitute.For<IBusinessIdeasRepository>();
        var handler = new DeleteBusinessIdeaOnUserDeleted(repository);

        await handler.Handle(new UserDeletedIntegrationEvent(Guid.NewGuid(), null), CancellationToken.None);

        await repository.DidNotReceive().DeleteForDreamAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
