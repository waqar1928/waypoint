using FluentValidation;
using MediatR;
using Waypoint.Common;

namespace Waypoint.Identity.Application.VerifyEmail;

public sealed record VerifyEmailCommand(Guid UserId, string Token) : IRequest;

public sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
    }
}

public sealed class VerifyEmailCommandHandler(IIdentityService identityService)
    : IRequestHandler<VerifyEmailCommand>
{
    public async Task Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);
        if (!result.Succeeded)
        {
            throw new ConflictException("This verification link is invalid or has expired.");
        }
    }
}
