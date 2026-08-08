using FluentValidation;
using MediatR;
using Waypoint.Common;

namespace Waypoint.Identity.Application.ResetPassword;

public sealed record ResetPasswordCommand(Guid UserId, string Token, string NewPassword) : IRequest;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(10)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");
    }
}

public sealed class ResetPasswordCommandHandler(IIdentityService identityService)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.ResetPasswordAsync(
            request.UserId, request.Token, request.NewPassword, cancellationToken);

        if (!result.Succeeded)
        {
            throw new ConflictException("This reset link is invalid or has expired.");
        }
    }
}
