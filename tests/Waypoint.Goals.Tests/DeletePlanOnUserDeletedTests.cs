using NSubstitute;
using Waypoint.Common;
using Waypoint.Goals.Application;
using Waypoint.Goals.Application.Registration;

namespace Waypoint.Goals.Tests;

public class DeletePlanOnUserDeletedTests
{
    [Fact]
    public async Task Deletes_the_plan_for_the_users_dream_when_the_account_is_deleted()
    {
        var repository = Substitute.For<IGoalsRepository>();
        var userId = Guid.NewGuid();
        var dreamId = Guid.NewGuid();
        var handler = new DeletePlanOnUserDeleted(repository);

        await handler.Handle(new UserDeletedIntegrationEvent(userId, dreamId), CancellationToken.None);

        await repository.Received(1).DeleteAllForDreamAsync(dreamId, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A user who never completed onboarding has no Dream and therefore nothing here to delete —
    /// must not throw or call the repository with a meaningless Guid.
    /// </summary>
    [Fact]
    public async Task Does_nothing_when_the_user_never_had_a_dream()
    {
        var repository = Substitute.For<IGoalsRepository>();
        var handler = new DeletePlanOnUserDeleted(repository);

        await handler.Handle(new UserDeletedIntegrationEvent(Guid.NewGuid(), null), CancellationToken.None);

        await repository.DidNotReceive().DeleteAllForDreamAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
