using Microsoft.Extensions.Options;
using NSubstitute;
using Waypoint.Identity.Application;
using Waypoint.Identity.Application.ResendVerification;
using Xunit;

namespace Waypoint.Identity.Tests;

public class ResendVerificationEmailCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IOptions<IdentityLinkOptions> _linkOptions =
        Options.Create(new IdentityLinkOptions { WebAppBaseUrl = "https://drevia.example" });

    private ResendVerificationEmailCommandHandler CreateHandler() =>
        new(_identityService, _emailSender, _linkOptions);

    [Fact]
    public async Task Existing_unconfirmed_account_gets_a_real_confirmation_email()
    {
        var userId = Guid.NewGuid();
        _identityService
            .GenerateEmailConfirmationTokenIfUnconfirmedAsync("alex@example.com", Arg.Any<CancellationToken>())
            .Returns((userId, "real-token"));

        await CreateHandler().Handle(new ResendVerificationEmailCommand("alex@example.com"), CancellationToken.None);

        await _emailSender.Received(1).SendAsync(
            "alex@example.com",
            Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains(userId.ToString()) && body.Contains("real-token")),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Anti-enumeration: whether the account doesn't exist or is already confirmed, this handler
    /// must behave identically from the caller's perspective — no email sent, no exception, no way
    /// to distinguish the two cases by response shape or timing-sensitive branching.
    /// </summary>
    [Fact]
    public async Task Nonexistent_or_already_confirmed_account_sends_no_email_and_does_not_throw()
    {
        _identityService
            .GenerateEmailConfirmationTokenIfUnconfirmedAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ValueTuple<Guid, string>?)null);

        await CreateHandler().Handle(new ResendVerificationEmailCommand("nobody@example.com"), CancellationToken.None);

        await _emailSender.DidNotReceive().SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
