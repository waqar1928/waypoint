using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Notifications.Application;
using Waypoint.Notifications.Application.GetMyNotifications;
using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Tests;

public class GetMyNotificationsQueryHandlerTests
{
    private readonly INotificationRepository _repository = Substitute.For<INotificationRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();

    private GetMyNotificationsQueryHandler CreateHandler() => new(_repository, _currentUser);

    [Fact]
    public async Task Returns_the_current_users_notifications_as_dtos()
    {
        _currentUser.UserId.Returns(_userId);
        var notification = new Notification
        {
            RecipientUserId = _userId,
            Category = NotificationCategories.MentorshipActivity,
            Title = "New response",
            Body = "A mentor replied",
            LinkUrl = "/app/mentorship",
        };
        _repository.GetForUserAsync(_userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([notification]);

        var result = await CreateHandler().Handle(new GetMyNotificationsQuery(), CancellationToken.None);

        result.Should().ContainSingle(n => n.Id == notification.Id && n.Title == "New response");
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(500, 200)]
    [InlineData(-5, 1)]
    public async Task Clamps_take_to_a_safe_range(int requested, int expectedClamped)
    {
        _currentUser.UserId.Returns(_userId);
        _repository.GetForUserAsync(_userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await CreateHandler().Handle(new GetMyNotificationsQuery(requested), CancellationToken.None);

        await _repository.Received(1).GetForUserAsync(_userId, expectedClamped, Arg.Any<CancellationToken>());
    }
}
