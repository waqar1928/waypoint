using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Notifications.Application.Push;
using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Tests;

public class SubscribeToPushCommandHandlerTests
{
    private readonly IPushSubscriptionRepository _repository = Substitute.For<IPushSubscriptionRepository>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly Guid _userId = Guid.NewGuid();

    private SubscribeToPushCommandHandler CreateHandler() => new(_repository, _currentUser);

    [Fact]
    public async Task Subscribes_using_the_authenticated_users_id_never_a_client_supplied_one()
    {
        _currentUser.UserId.Returns(_userId);
        var created = PushSubscription.Create(_userId, "https://fcm.googleapis.com/fcm/send/abc", "p256dh", "auth", "Chrome");
        _repository.UpsertAsync(_userId, "https://fcm.googleapis.com/fcm/send/abc", "p256dh", "auth", "Chrome", Arg.Any<CancellationToken>())
            .Returns(created);

        var command = new SubscribeToPushCommand("https://fcm.googleapis.com/fcm/send/abc", "p256dh", "auth", "Chrome");
        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.Id.Should().Be(created.Id);
        // The command itself has no UserId field at all - this assertion documents that, rather
        // than merely trusting it: the handler can only ever have called UpsertAsync with the
        // value it read from ICurrentUserAccessor.
        await _repository.Received(1).UpsertAsync(
            _userId, "https://fcm.googleapis.com/fcm/send/abc", "p256dh", "auth", "Chrome", Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Validator_rejects_a_non_HTTPS_endpoint()
    {
        var validator = new SubscribeToPushCommandValidator();
        var command = new SubscribeToPushCommand("http://fcm.googleapis.com/fcm/send/abc", "p256dh", "auth", null);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_rejects_an_IP_literal_endpoint()
    {
        var validator = new SubscribeToPushCommandValidator();
        var command = new SubscribeToPushCommand("https://169.254.169.254/metadata", "p256dh", "auth", null);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_accepts_a_well_formed_push_endpoint()
    {
        var validator = new SubscribeToPushCommandValidator();
        var command = new SubscribeToPushCommand("https://fcm.googleapis.com/fcm/send/abc", "p256dh", "auth", "Chrome");

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
