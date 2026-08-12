using MediatR;
using Waypoint.Common;

namespace Waypoint.Notifications.Application.GetMyNotifications;

public sealed record GetMyNotificationsQuery(int Take = 50) : IRequest<IReadOnlyList<NotificationDto>>;

public sealed class GetMyNotificationsQueryHandler(INotificationRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    public async Task<IReadOnlyList<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var take = Math.Clamp(request.Take, 1, 200);
        var notifications = await repository.GetForUserAsync(userId, take, cancellationToken);
        return notifications.Select(NotificationDto.From).ToList();
    }
}
