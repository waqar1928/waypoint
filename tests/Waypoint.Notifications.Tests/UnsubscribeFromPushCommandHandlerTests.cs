using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Notifications.Application.Push;
using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Tests;

public class UnsubscribeFromPushCommandHandlerTests
{
    private readonly IPushSubscriptionRepository _repository = Substitute.For<IPushSubscriptionRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();

    private UnsubscribeFromPushCommandHandler CreateHandler() => new(_repository, _currentUser);

    [Fact]
    public async Task Deactivates_the_callers_own_active_subscription()
    {
        _currentUser.UserId.Returns(_userId);
        var subscription = PushSubscription.Create(_userId, "https://fcm.googleapis.com/fcm/send/abc", "p256dh", "auth", null);
        _repository.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);

        await CreateHandler().Handle(new UnsubscribeFromPushCommand(subscription.Id), CancellationToken.None);

        await _repository.Received(1).DeactivateAsync(subscription, "UserUnsubscribed", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Is_idempotent_for_an_already_deactivated_subscription()
    {
        _currentUser.UserId.Returns(_userId);
        var subscription = PushSubscription.Create(_userId, "https://fcm.googleapis.com/fcm/send/abc", "p256dh", "auth", null);
        subscription.Status = PushSubscriptionStatus.Deactivated;
        _repository.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);

        await CreateHandler().Handle(new UnsubscribeFromPushCommand(subscription.Id), CancellationToken.None);

        await _repository.DidNotReceive().DeactivateAsync(Arg.Any<PushSubscription>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    /// <summary>Same anti-enumeration reasoning as every other ownership check in this codebase
    /// (see MarkNotificationReadCommandHandlerTests): NotFoundException, never a 403-shaped
    /// exception, so a mismatched owner can't distinguish "doesn't exist" from "exists but isn't
    /// yours."</summary>
    [Fact]
    public async Task Throws_not_found_for_someone_elses_subscription()
    {
        _currentUser.UserId.Returns(_userId);
        var subscription = PushSubscription.Create(Guid.NewGuid(), "https://fcm.googleapis.com/fcm/send/abc", "p256dh", "auth", null);
        _repository.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);

        var act = () => CreateHandler().Handle(new UnsubscribeFromPushCommand(subscription.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        await _repository.DidNotReceive().DeactivateAsync(Arg.Any<PushSubscription>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_not_found_for_a_nonexistent_subscription()
    {
        _currentUser.UserId.Returns(_userId);
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PushSubscription?)null);

        var act = () => CreateHandler().Handle(new UnsubscribeFromPushCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
