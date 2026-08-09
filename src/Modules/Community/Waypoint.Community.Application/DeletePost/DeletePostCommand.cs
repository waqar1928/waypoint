using MediatR;
using Waypoint.Common;

namespace Waypoint.Community.Application.DeletePost;

public sealed record DeletePostCommand(Guid PostId) : IRequest;

public sealed class DeletePostCommandHandler(ICommunityRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<DeletePostCommand>
{
    public async Task Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var post = await repository.GetPostByIdAsync(request.PostId, cancellationToken)
            ?? throw new NotFoundException("Post not found.");

        if (post.UserId != userId)
        {
            throw new NotFoundException("Post not found.");
        }

        post.DeletedAt = DateTimeOffset.UtcNow;
        post.UpdatedBy = userId;
        await repository.SavePostAsync(post, cancellationToken);
    }
}
