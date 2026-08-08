using FluentValidation;
using MediatR;
using Waypoint.Common;

namespace Waypoint.Identity.Application.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResult>;

/// <summary>
/// OnboardingCompleted is hardcoded false in Phase 1 — the Dream/Users
/// onboarding flow doesn't exist yet (see docs/09-phased-plan.md, Phase 2).
/// The field stays on the contract now so the frontend doesn't need a
/// breaking change when onboarding tracking lands.
/// </summary>
public sealed record LoginResult(Guid UserId, string Email, bool OnboardingCompleted);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(IIdentityService identityService, IAuditSink auditSink)
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
            return new LoginResult(result.UserId!.Value, result.Email!, OnboardingCompleted: false);
        }

        await auditSink.RecordAsync(
            new AuditEntry("User", Guid.Empty, "LoginFailed", null, $"email={request.Email}", DateTimeOffset.UtcNow),
            cancellationToken);

        throw result.Outcome switch
        {
            SignInOutcome.LockedOut => new AccountLockedException("Too many failed attempts. Please try again later."),
            _ => new AuthenticationFailedException("Incorrect email or password."),
        };
    }
}
