using NSubstitute;
using Waypoint.Common;
using Waypoint.Journal.Application;
using Waypoint.Journal.Application.Registration;

namespace Waypoint.Journal.Tests;

public class DeleteJournalEntriesOnUserDeletedTests
{
    [Fact]
    public async Task Deletes_all_journal_entries_for_the_user_when_the_account_is_deleted()
    {
        var repository = Substitute.For<IJournalRepository>();
        var userId = Guid.NewGuid();
        var handler = new DeleteJournalEntriesOnUserDeleted(repository);

        await handler.Handle(new UserDeletedIntegrationEvent(userId, null), CancellationToken.None);

        await repository.Received(1).DeleteAllForUserAsync(userId, Arg.Any<CancellationToken>());
    }
}
