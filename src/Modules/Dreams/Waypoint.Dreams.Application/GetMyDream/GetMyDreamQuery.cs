using MediatR;
using Waypoint.Common;
using Waypoint.Dreams.Application.SelectDreamDirection;

namespace Waypoint.Dreams.Application.GetMyDream;

public sealed record GetMyDreamQuery : IRequest<DreamDto?>;

public sealed class GetMyDreamQueryHandler(IDreamRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyDreamQuery, DreamDto?>
{
    public async Task<DreamDto?> Handle(GetMyDreamQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var dream = await repository.GetByUserIdAsync(userId, cancellationToken);
        return dream is null ? null : SelectDreamDirectionCommandHandler.ToDto(dream);
    }
}
