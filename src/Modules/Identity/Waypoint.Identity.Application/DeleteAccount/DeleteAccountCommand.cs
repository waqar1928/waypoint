using FluentValidation;
using MediatR;
using Waypoint.Common;

namespace Waypoint.Identity.Application.DeleteAccount;

public sealed record DeleteAccountCommand(string Password) : IRequest;

public sealed class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class DeleteAccountCommandHandler(
    IIdentityService identityService, ICurrentUserAccessor currentUser, IPublisher publisher)
    : IRequestHandler<DeleteAccountCommand>
{
    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        var passwordValid = await identityService.CheckPasswordAsync(userId, request.Password, cancellationToken);
        if (!passwordValid)
        {
            throw new AuthenticationFailedException("Incorrect password.");
        }

        await identityService.SignOutAsync(cancellationToken);

        var result = await identityService.DeleteUserAsync(userId, cancellationToken);
        if (!result.Succeeded)
        {
            throw new ConflictException("We couldn't delete your account. Please try again.");
        }

        await publisher.Publish(new UserDeletedIntegrationEvent(userId), cancellationToken);
    }
}
