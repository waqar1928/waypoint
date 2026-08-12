using NSubstitute;
using Waypoint.Common;
using Waypoint.Mentorship.Application;
using Waypoint.Mentorship.Application.Registration;

namespace Waypoint.Mentorship.Tests;

public class DeleteMentorshipDataOnUserDeletedTests
{
    [Fact]
    public async Task Deletes_all_mentorship_data_for_the_user_when_the_account_is_deleted()
    {
        var repository = Substitute.For<IMentorshipRepository>();
        var userId = Guid.NewGuid();
        var handler = new DeleteMentorshipDataOnUserDeleted(repository);

        await handler.Handle(new UserDeletedIntegrationEvent(userId, null), CancellationToken.None);

        await repository.Received(1).DeleteAllForUserAsync(userId, Arg.Any<CancellationToken>());
    }
}
