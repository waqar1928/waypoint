using FluentValidation;
using MediatR;

namespace Waypoint.Identity.Application.Register;

public sealed record RegisterUserCommand(string DisplayName, string Email, string Password)
    : IRequest<RegisterUserResult>;

public sealed record RegisterUserResult(Guid UserId, string Email, bool EmailConfirmationSent);

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(10)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");
    }
}
