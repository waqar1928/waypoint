using FluentAssertions;
using MediatR;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Identity.Application;
using Waypoint.Identity.Application.DeleteAccount;
using Xunit;

namespace Waypoint.Identity.Tests;

public class DeleteAccountCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ICurrentUserAccessor _currentUser = Substitute.For<ICurrentUserAccessor>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly Guid _userId = Guid.NewGuid();

    private DeleteAccountCommandHandler CreateHandler() => new(_identityService, _currentUser, _publisher);

    [Fact]
    public async Task Deletes_the_account_signs_out_and_publishes_the_event_when_the_password_is_correct()
    {
        _currentUser.UserId.Returns(_userId);
        _identityService.CheckPasswordAsync(_userId, "correct-password", Arg.Any<CancellationToken>()).Returns(true);
        _identityService.DeleteUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(IdentityOperationResult.Success());

        await CreateHandler().Handle(new DeleteAccountCommand("correct-password"), CancellationToken.None);

        await _identityService.Received(1).SignOutAsync(Arg.Any<CancellationToken>());
        await _identityService.Received(1).DeleteUserAsync(_userId, Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(
            Arg.Is<UserDeletedIntegrationEvent>(e => e.UserId == _userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_and_never_deletes_when_the_password_is_wrong()
    {
        _currentUser.UserId.Returns(_userId);
        _identityService.CheckPasswordAsync(_userId, "wrong-password", Arg.Any<CancellationToken>()).Returns(false);

        var act = () => CreateHandler().Handle(new DeleteAccountCommand("wrong-password"), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
        await _identityService.DidNotReceive().DeleteUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _identityService.DidNotReceive().SignOutAsync(Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<UserDeletedIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_conflict_when_the_underlying_delete_fails_after_a_correct_password()
    {
        _currentUser.UserId.Returns(_userId);
        _identityService.CheckPasswordAsync(_userId, "correct-password", Arg.Any<CancellationToken>()).Returns(true);
        _identityService.DeleteUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(IdentityOperationResult.Failure(["Something went wrong."]));

        var act = () => CreateHandler().Handle(new DeleteAccountCommand("correct-password"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _publisher.DidNotReceive().Publish(Arg.Any<UserDeletedIntegrationEvent>(), Arg.Any<CancellationToken>());
    }
}
