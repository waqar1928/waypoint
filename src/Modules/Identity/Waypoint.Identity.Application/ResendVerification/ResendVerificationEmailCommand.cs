using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace Waypoint.Identity.Application.ResendVerification;

public sealed record ResendVerificationEmailCommand(string Email) : IRequest;

public sealed class ResendVerificationEmailCommandValidator : AbstractValidator<ResendVerificationEmailCommand>
{
    public ResendVerificationEmailCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

/// <summary>
/// Always returns successfully regardless of whether the email exists or is already confirmed —
/// same anti-enumeration shape as ForgotPasswordCommandHandler, for the same reason (callers must
/// not be able to tell account existence or confirmation state apart from this endpoint's
/// response).
/// </summary>
public sealed class ResendVerificationEmailCommandHandler(
    IIdentityService identityService,
    IEmailSender emailSender,
    IOptions<IdentityLinkOptions> linkOptions)
    : IRequestHandler<ResendVerificationEmailCommand>
{
    public async Task Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var outcome = await identityService.GenerateEmailConfirmationTokenIfUnconfirmedAsync(request.Email, cancellationToken);
        if (outcome is not { } found)
        {
            return;
        }

        var verificationLink =
            $"{linkOptions.Value.WebAppBaseUrl}/verify-email?userId={found.UserId}&token={Uri.EscapeDataString(found.Token)}";

        await emailSender.SendAsync(
            request.Email,
            "Confirm your Waypoint account",
            $"""<p>Confirm your email to log in to Waypoint:</p><p><a href="{verificationLink}">Confirm email</a></p>""",
            cancellationToken);
    }
}
