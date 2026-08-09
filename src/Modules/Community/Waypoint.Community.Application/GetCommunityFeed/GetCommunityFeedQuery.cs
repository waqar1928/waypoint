using MediatR;
using Waypoint.Common;

namespace Waypoint.Community.Application.GetCommunityFeed;

public sealed record GetCommunityFeedQuery(int Take = 100) : IRequest<IReadOnlyList<PostDto>>;

public sealed class GetCommunityFeedQueryHandler(
    ICommunityRepository repository, IProfileSummaryProvider profileSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetCommunityFeedQuery, IReadOnlyList<PostDto>>
{
    public async Task<IReadOnlyList<PostDto>> Handle(GetCommunityFeedQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        var take = Math.Clamp(request.Take, 1, 200);
        var posts = await repository.GetFeedAsync(take, cancellationToken);
        var counts = await repository.GetCommentCountsAsync(posts.Select(p => p.Id).ToList(), cancellationToken);
        var authors = await AuthorResolver.ResolveManyAsync(
            profileSummaryProvider, posts.Select(p => p.UserId).ToList(), cancellationToken);

        // Already ordered by the repository (most-recent-first, capped at `take`); the DTO
        // projection just needs to preserve that order, not re-sort.
        return posts
            .Select(p => PostDto.From(p, authors[p.UserId], counts.GetValueOrDefault(p.Id), userId))
            .ToList();
    }
}
