using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;

namespace Waypoint.Identity.Application.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

/// <summary>
/// Always returns successfully regardless of whether the email exists —
/// callers must not be able to tell account existence apart from this
/// endpoint's response (see docs/05-api-contract.md, forgot-password).
/// </summary>
public sealed class ForgotPasswordCommandHandler(
    IIdentityService identityService,
    IEmailSender emailSender,
    IOptions<IdentityLinkOptions> linkOptions)
    : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var outcome = await identityService.GeneratePasswordResetTokenIfExistsAsync(request.Email, cancellationToken);
        if (outcome is not { } found)
        {
            return;
        }

        var resetLink =
            $"{linkOptions.Value.WebAppBaseUrl}/reset-password?userId={found.UserId}&token={Uri.EscapeDataString(found.Token)}";

        await emailSender.SendAsync(
            request.Email,
            "Reset your Waypoint password",
            $"""<p>Reset your password:</p><p><a href="{resetLink}">Reset password</a></p>""",
            cancellationToken);
    }
}
