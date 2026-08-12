using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Notifications.Application;
using Waypoint.Notifications.Application.MarkAsRead;
using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Tests;

public class MarkNotificationReadCommandHandlerTests
{
    private readonly INotificationRepository _repository = Substitute.For<INotificationRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();

    private MarkNotificationReadCommandHandler CreateHandler() => new(_repository, _currentUser);

    [Fact]
    public async Task Marks_own_unread_notification_as_read()
    {
        _currentUser.UserId.Returns(_userId);
        var notification = new Notification
        {
            RecipientUserId = _userId,
            Category = NotificationCategories.CommunityActivity,
            Title = "New comment",
            Body = "Someone replied",
        };
        _repository.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>()).Returns(notification);

        await CreateHandler().Handle(new MarkNotificationReadCommand(notification.Id), CancellationToken.None);

        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().NotBeNull();
        await _repository.Received(1).SaveAsync(notification, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Is_idempotent_and_does_not_resave_an_already_read_notification()
    {
        _currentUser.UserId.Returns(_userId);
        var notification = new Notification
        {
            RecipientUserId = _userId,
            Category = NotificationCategories.CommunityActivity,
            Title = "New comment",
            Body = "Someone replied",
            IsRead = true,
            ReadAt = DateTimeOffset.UtcNow.AddMinutes(-5),
        };
        _repository.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>()).Returns(notification);

        await CreateHandler().Handle(new MarkNotificationReadCommand(notification.Id), CancellationToken.None);

        await _repository.DidNotReceive().SaveAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Regression test for the same ownership-bypass class of bug covered everywhere else in this
    /// codebase (see docs/PRODUCTION_READINESS_AUDIT.md Authorization section) — a user must never
    /// be able to mark, and therefore never even confirm the existence of, another user's
    /// notification. NotFoundException specifically (not a 403-shaped exception), so a mismatched
    /// owner can't distinguish "doesn't exist" from "exists but isn't yours".
    /// </summary>
    [Fact]
    public async Task Throws_not_found_for_someone_elses_notification()
    {
        _currentUser.UserId.Returns(_userId);
        var notification = new Notification
        {
            RecipientUserId = Guid.NewGuid(),
            Category = NotificationCategories.CommunityActivity,
            Title = "New comment",
            Body = "Someone replied",
        };
        _repository.GetByIdAsync(notification.Id, Arg.Any<CancellationToken>()).Returns(notification);

        var act = () => CreateHandler().Handle(new MarkNotificationReadCommand(notification.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _repository.DidNotReceive().SaveAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_not_found_for_a_nonexistent_notification()
    {
        _currentUser.UserId.Returns(_userId);
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Notification?)null);

        var act = () => CreateHandler().Handle(new MarkNotificationReadCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
