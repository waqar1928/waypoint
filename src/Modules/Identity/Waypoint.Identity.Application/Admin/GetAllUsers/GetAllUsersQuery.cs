using MediatR;
using Waypoint.Common;

namespace Waypoint.Identity.Application.Admin.GetAllUsers;

public sealed record GetAllUsersQuery : IRequest<IReadOnlyList<AdminUserDto>>;

public sealed class GetAllUsersQueryHandler(IIdentityService identityService, IProfileSummaryProvider profileSummaryProvider)
    : IRequestHandler<GetAllUsersQuery, IReadOnlyList<AdminUserDto>>
{
    public async Task<IReadOnlyList<AdminUserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await identityService.GetAllUsersAsync(cancellationToken);
        var profiles = await profileSummaryProvider.GetForUsersAsync(users.Select(u => u.Id).ToList(), cancellationToken);

        return users
            .Select(u => new AdminUserDto(
                u.Id, u.Email, profiles.TryGetValue(u.Id, out var p) ? p.DisplayName : null,
                u.EmailConfirmed, u.IsLockedOut, u.LockoutEnd))
            .ToList();
    }
}
