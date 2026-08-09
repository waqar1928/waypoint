using MediatR;
using Waypoint.Common;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Application.GetPostComments;

public sealed record GetPostCommentsQuery(Guid PostId) : IRequest<IReadOnlyList<CommentDto>>;

public sealed class GetPostCommentsQueryHandler(
    ICommunityRepository repository, IProfileSummaryProvider profileSummaryProvider, ICurrentUserAccessor currentUser)
    : IRequestHandler<GetPostCommentsQuery, IReadOnlyList<CommentDto>>
{
    public async Task<IReadOnlyList<CommentDto>> Handle(GetPostCommentsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var post = await repository.GetPostByIdAsync(request.PostId, cancellationToken)
            ?? throw new NotFoundException("Post not found.");

        if (post.Visibility == PostVisibility.Private && post.UserId != userId)
        {
            throw new NotFoundException("Post not found.");
        }

        var comments = await repository.GetCommentsForPostAsync(request.PostId, cancellationToken);
        var authors = await AuthorResolver.ResolveManyAsync(
            profileSummaryProvider, comments.Select(c => c.UserId).ToList(), cancellationToken);

        return comments
            .OrderBy(c => c.CreatedAt)
            .Select(c => CommentDto.From(c, authors[c.UserId], userId))
            .ToList();
    }
}
