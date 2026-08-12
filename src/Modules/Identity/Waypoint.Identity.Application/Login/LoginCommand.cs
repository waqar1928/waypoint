using FluentValidation;
using MediatR;
using Waypoint.Common;

namespace Waypoint.Identity.Application.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

public sealed record LoginResult(Guid UserId, string Email, bool OnboardingCompleted);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(
    IIdentityService identityService, IAuditSink auditSink, IOnboardingStatusProvider onboardingStatus)
    : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.PasswordSignInAsync(request.Email, request.Password, cancellationToken);

        if (result.Outcome == SignInOutcome.Success)
        {
            await auditSink.RecordAsync(
                new AuditEntry("User", result.UserId!.Value, "LoginSucceeded", result.UserId, null, DateTimeOffset.UtcNow),
                cancellationToken);
            var onboardingCompleted = await onboardingStatus.HasCompletedOnboardingAsync(result.UserId!.Value, cancellationToken);
            return new LoginResult(result.UserId!.Value, result.Email!, onboardingCompleted);
        }

        await auditSink.RecordAsync(
            new AuditEntry("User", Guid.Empty, "LoginFailed", null, $"email={request.Email}", DateTimeOffset.UtcNow),
            cancellationToken);

        throw result.Outcome switch
        {
            SignInOutcome.LockedOut => new AccountLockedException("Too many failed attempts. Please try again later."),
            SignInOutcome.EmailNotConfirmed => new EmailNotConfirmedException(
                "Please confirm your email address before logging in. Check your inbox, or request a new confirmation email."),
            _ => new AuthenticationFailedException("Incorrect email or password."),
        };
    }
}
