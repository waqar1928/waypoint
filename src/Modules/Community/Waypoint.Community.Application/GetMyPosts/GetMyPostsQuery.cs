using MediatR;
using Waypoint.Common;

namespace Waypoint.Community.Application.GetMyPosts;

public sealed record GetMyPostsQuery : IRequest<IReadOnlyList<PostDto>>;

public sealed class GetMyPostsQueryHandler(
    ICommunityRepository repository,
    IProfileSummaryProvider profileSummaryProvider,
    IDreamSummaryProvider dreamSummaryProvider,
    ICurrentUserAccessor currentUser)
    : IRequestHandler<GetMyPostsQuery, IReadOnlyList<PostDto>>
{
    public async Task<IReadOnlyList<PostDto>> Handle(GetMyPostsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");

        var posts = await repository.GetPostsForUserAsync(userId, cancellationToken);
        var counts = await repository.GetCommentCountsAsync(posts.Select(p => p.Id).ToList(), cancellationToken);
        var author = await AuthorResolver.ResolveAsync(profileSummaryProvider, userId, cancellationToken);
        var attachedDreams = await AttachedDreamResolver.ResolveManyAsync(
            dreamSummaryProvider,
            posts.Where(p => p.DreamId.HasValue).Select(p => p.DreamId!.Value).ToList(),
            cancellationToken);

        return posts
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => PostDto.From(
                p, author, p.DreamId.HasValue ? attachedDreams.GetValueOrDefault(p.DreamId.Value) : null,
                counts.GetValueOrDefault(p.Id), userId))
            .ToList();
    }
}
