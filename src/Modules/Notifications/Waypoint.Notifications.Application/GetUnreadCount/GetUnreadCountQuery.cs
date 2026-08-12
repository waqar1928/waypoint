using MediatR;
using Waypoint.Common;

namespace Waypoint.Notifications.Application.GetUnreadCount;

public sealed record GetUnreadCountQuery : IRequest<int>;

public sealed class GetUnreadCountQueryHandler(INotificationRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetUnreadCountQuery, int>
{
    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        return await repository.GetUnreadCountAsync(userId, cancellationToken);
    }
}
