using MediatR;
using Waypoint.Common;

namespace Waypoint.Mentorship.Application.GetMyHelpRequests;

public sealed record GetMyHelpRequestsQuery : IRequest<IReadOnlyList<HelpRequestDto>>;

public sealed class GetMyHelpRequestsQueryHandler(
    IMentorshipRepository repository,
    IProfileSummaryProvider profileSummaryProvider,
    IDreamSummaryProvider dreamSummaryProvider,
    ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyHelpRequestsQuery, IReadOnlyList<HelpRequestDto>>
{
    public async Task<IReadOnlyList<HelpRequestDto>> Handle(GetMyHelpRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        var requests = await repository.GetHelpRequestsForUserAsync(userId, cancellationToken);
        var counts = await repository.GetResponseCountsAsync(requests.Select(r => r.Id).ToList(), cancellationToken);
        var author = await PersonResolver.ResolveAsync(profileSummaryProvider, userId, cancellationToken);
        var attachedDreams = await AttachedDreamResolver.ResolveManyAsync(
            dreamSummaryProvider,
            requests.Where(r => r.DreamId.HasValue).Select(r => r.DreamId!.Value).ToList(),
            cancellationToken);

        return requests
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => HelpRequestDto.From(
                r, author, r.DreamId.HasValue ? attachedDreams.GetValueOrDefault(r.DreamId.Value) : null,
                counts.GetValueOrDefault(r.Id), userId))
            .ToList();
    }
}
