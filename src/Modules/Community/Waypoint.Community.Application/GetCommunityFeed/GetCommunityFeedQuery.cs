using MediatR;
using Waypoint.Common;

namespace Waypoint.Community.Application.GetCommunityFeed;

public sealed record GetCommunityFeedQuery : IRequest<IReadOnlyList<PostDto>>;

public sealed class GetCommunityFeedQueryHandler(
    ICommunityRepository repository, IProfileSummaryProvider profileSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetCommunityFeedQuery, IReadOnlyList<PostDto>>
{
    public async Task<IReadOnlyList<PostDto>> Handle(GetCommunityFeedQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        var posts = await repository.GetFeedAsync(cancellationToken);
        var counts = await repository.GetCommentCountsAsync(posts.Select(p => p.Id).ToList(), cancellationToken);
        var authors = await AuthorResolver.ResolveManyAsync(
            profileSummaryProvider, posts.Select(p => p.UserId).ToList(), cancellationToken);

        return posts
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => PostDto.From(p, authors[p.UserId], counts.GetValueOrDefault(p.Id), userId))
            .ToList();
    }
}
