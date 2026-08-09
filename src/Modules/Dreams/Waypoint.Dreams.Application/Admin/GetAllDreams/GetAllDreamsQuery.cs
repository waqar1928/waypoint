using MediatR;
using Waypoint.Common;
using Waypoint.Dreams.Domain;

namespace Waypoint.Dreams.Application.Admin.GetAllDreams;

public sealed record AdminDreamDto(
    Guid Id,
    Guid UserId,
    string? OwnerDisplayName,
    string Title,
    DreamStage Stage,
    bool IsBusinessShaped,
    DateTimeOffset CreatedAt);

public sealed record GetAllDreamsQuery : IRequest<IReadOnlyList<AdminDreamDto>>;

public sealed class GetAllDreamsQueryHandler(IDreamRepository repository, IProfileSummaryProvider profileSummaryProvider)
    : IRequestHandler<GetAllDreamsQuery, IReadOnlyList<AdminDreamDto>>
{
    public async Task<IReadOnlyList<AdminDreamDto>> Handle(GetAllDreamsQuery request, CancellationToken cancellationToken)
    {
        var dreams = await repository.GetAllAsync(cancellationToken);
        var profiles = await profileSummaryProvider.GetForUsersAsync(dreams.Select(d => d.UserId).ToList(), cancellationToken);

        return dreams
            .Select(d => new AdminDreamDto(
                d.Id, d.UserId, profiles.TryGetValue(d.UserId, out var p) ? p.DisplayName : null,
                d.Title, d.Stage, d.IsBusinessShaped, d.CreatedAt))
            .ToList();
    }
}
