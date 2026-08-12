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
    private readonly IAuditSink _auditSink = Substitute.For<IAuditSink>();
    private readonly IDreamSummaryProvider _dreamSummaryProvider = Substitute.For<IDreamSummaryProvider>();
    private readonly Guid _userId = Guid.NewGuid();

    private DeleteAccountCommandHandler CreateHandler() =>
        new(_identityService, _currentUser, _publisher, _auditSink, _dreamSummaryProvider);

    [Fact]
    public async Task Deletes_the_account_signs_out_publishes_the_event_and_records_an_audit_entry_when_the_password_is_correct()
    {
        _currentUser.UserId.Returns(_userId);
        _identityService.CheckPasswordAsync(_userId, "correct-password", Arg.Any<CancellationToken>()).Returns(true);
        _identityService.DeleteUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(IdentityOperationResult.Success());

        await CreateHandler().Handle(new DeleteAccountCommand("correct-password"), CancellationToken.None);

        await _identityService.Received(1).SignOutAsync(Arg.Any<CancellationToken>());
        await _identityService.Received(1).DeleteUserAsync(_userId, Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(
            Arg.Is<UserDeletedIntegrationEvent>(e => e.UserId == _userId), Arg.Any<CancellationToken>());
        await _auditSink.Received(1).RecordAsync(
            Arg.Is<AuditEntry>(e => e.Action == "AccountDeleted" && e.ActorUserId == _userId), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Regression test for the cascade-deletion pass — every module that only keys off DreamId
    /// (Goals/Actions/Experiments/BusinessIdeas) depends on this event carrying the real DreamId,
    /// resolved *before* the account is gone (see UserDeletedIntegrationEvent's doc comment for
    /// why a live lookup during cascade-delete handling would be unsafe).
    /// </summary>
    [Fact]
    public async Task Publishes_the_users_dream_id_on_the_deletion_event_resolved_before_the_account_is_deleted()
    {
        var dreamId = Guid.NewGuid();
        _currentUser.UserId.Returns(_userId);
        _identityService.CheckPasswordAsync(_userId, "correct-password", Arg.Any<CancellationToken>()).Returns(true);
        _identityService.DeleteUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(IdentityOperationResult.Success());
        _dreamSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(new DreamSummary(dreamId, _userId, "My dream", "Statement", null, null, null, null, null, null, false));

        await CreateHandler().Handle(new DeleteAccountCommand("correct-password"), CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<UserDeletedIntegrationEvent>(e => e.DreamId == dreamId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Publishes_a_null_dream_id_when_the_user_never_completed_onboarding()
    {
        _currentUser.UserId.Returns(_userId);
        _identityService.CheckPasswordAsync(_userId, "correct-password", Arg.Any<CancellationToken>()).Returns(true);
        _identityService.DeleteUserAsync(_userId, Arg.Any<CancellationToken>()).Returns(IdentityOperationResult.Success());
        _dreamSummaryProvider.GetForUserAsync(_userId, Arg.Any<CancellationToken>()).Returns((DreamSummary?)null);

        await CreateHandler().Handle(new DeleteAccountCommand("correct-password"), CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<UserDeletedIntegrationEvent>(e => e.DreamId == null), Arg.Any<CancellationToken>());
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
        await _auditSink.DidNotReceive().RecordAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_conflict_and_does_not_audit_when_the_underlying_delete_fails_after_a_correct_password()
    {
        _currentUser.UserId.Returns(_userId);
        _identityService.CheckPasswordAsync(_userId, "correct-password", Arg.Any<CancellationToken>()).Returns(true);
        _identityService.DeleteUserAsync(_userId, Arg.Any<CancellationToken>())
            .Returns(IdentityOperationResult.Failure(["Something went wrong."]));

        var act = () => CreateHandler().Handle(new DeleteAccountCommand("correct-password"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _publisher.DidNotReceive().Publish(Arg.Any<UserDeletedIntegrationEvent>(), Arg.Any<CancellationToken>());
        await _auditSink.DidNotReceive().RecordAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }
}
