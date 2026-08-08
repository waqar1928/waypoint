using FluentAssertions;
using NSubstitute;
using Waypoint.Common;
using Waypoint.Identity.Application;
using Waypoint.Identity.Application.Login;
using Xunit;

namespace Waypoint.Identity.Tests;

public class LoginCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IAuditSink _auditSink = Substitute.For<IAuditSink>();
    private readonly IOnboardingStatusProvider _onboardingStatus = Substitute.For<IOnboardingStatusProvider>();

    private LoginCommandHandler CreateHandler() => new(_identityService, _auditSink, _onboardingStatus);

    [Fact]
    public async Task Successful_sign_in_returns_login_result_and_records_audit_entry()
    {
        var userId = Guid.NewGuid();
        _identityService
            .PasswordSignInAsync("alex@example.com", "correct", Arg.Any<CancellationToken>())
            .Returns(new PasswordSignInResult(SignInOutcome.Success, userId, "alex@example.com"));
        _onboardingStatus.HasCompletedOnboardingAsync(userId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await CreateHandler().Handle(
            new LoginCommand("alex@example.com", "correct"), CancellationToken.None);

        result.UserId.Should().Be(userId);
        result.Email.Should().Be("alex@example.com");
        result.OnboardingCompleted.Should().BeTrue();
        await _auditSink.Received(1).RecordAsync(
            Arg.Is<AuditEntry>(e => e.Action == "LoginSucceeded"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Invalid_credentials_throws_authentication_failed_and_records_audit_entry()
    {
        _identityService
            .PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new PasswordSignInResult(SignInOutcome.InvalidCredentials, null, null));

        var act = () => CreateHandler().Handle(
            new LoginCommand("alex@example.com", "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<AuthenticationFailedException>();
        await _auditSink.Received(1).RecordAsync(
            Arg.Is<AuditEntry>(e => e.Action == "LoginFailed"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Locked_out_account_throws_account_locked_exception()
    {
        _identityService
            .PasswordSignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new PasswordSignInResult(SignInOutcome.LockedOut, null, null));

        var act = () => CreateHandler().Handle(
            new LoginCommand("alex@example.com", "wrong"), CancellationToken.None);

        await act.Should().ThrowAsync<AccountLockedException>();
    }
}
