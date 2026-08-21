using FluentValidation;
using MediatR;
using Waypoint.Common;

namespace Waypoint.Notifications.Application.Push;

public sealed record SubscribeToPushCommand(string Endpoint, string P256dh, string Auth, string? UserAgent)
    : IRequest<PushSubscriptionDto>;

public sealed class SubscribeToPushCommandValidator : AbstractValidator<SubscribeToPushCommand>
{
    public SubscribeToPushCommandValidator()
    {
        RuleFor(x => x.Endpoint)
            .NotEmpty()
            .Must(EndpointSafety.IsWellFormedHttpsEndpoint)
            .WithMessage("That doesn't look like a valid push subscription endpoint.");
        RuleFor(x => x.P256dh).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Auth).NotEmpty().MaximumLength(255);
        RuleFor(x => x.UserAgent).MaximumLength(500);
    }
}

/// <summary>UserId always comes from the authenticated principal, never from the request body -
/// there is no client-supplied UserId field on this command at all.</summary>
public sealed class SubscribeToPushCommandHandler(IPushSubscriptionRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<SubscribeToPushCommand, PushSubscriptionDto>
{
    public async Task<PushSubscriptionDto> Handle(SubscribeToPushCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var subscription = await repository.UpsertAsync(
            userId, request.Endpoint, request.P256dh, request.Auth, request.UserAgent, cancellationToken);
        return PushSubscriptionDto.From(subscription);
    }
}
