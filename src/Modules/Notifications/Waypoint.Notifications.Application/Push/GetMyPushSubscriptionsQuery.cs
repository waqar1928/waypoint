using MediatR;
using Waypoint.Common;

namespace Waypoint.Notifications.Application.Push;

/// <summary>Backs a future "manage your notification devices" list in Settings - every
/// subscription regardless of status, so a user can see (and remove) stale ones too.</summary>
public sealed record GetMyPushSubscriptionsQuery : IRequest<IReadOnlyList<PushSubscriptionDto>>;

public sealed class GetMyPushSubscriptionsQueryHandler(IPushSubscriptionRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyPushSubscriptionsQuery, IReadOnlyList<PushSubscriptionDto>>
{
    public async Task<IReadOnlyList<PushSubscriptionDto>> Handle(
        GetMyPushSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var subscriptions = await repository.GetAllForUserAsync(userId, cancellationToken);
        return subscriptions.Select(PushSubscriptionDto.From).ToList();
    }
}
