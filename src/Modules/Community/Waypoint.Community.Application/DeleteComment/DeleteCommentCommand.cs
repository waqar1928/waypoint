using MediatR;
using Waypoint.Common;

namespace Waypoint.Community.Application.DeleteComment;

public sealed record DeleteCommentCommand(Guid CommentId) : IRequest;

public sealed class DeleteCommentCommandHandler(ICommunityRepository repository, ICurrentUserAccessor currentUser)
    : IRequestHandler<DeleteCommentCommand>
{
    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var comment = await repository.GetCommentByIdAsync(request.CommentId, cancellationToken)
            ?? throw new NotFoundException("Comment not found.");

        if (comment.UserId != userId)
        {
            throw new NotFoundException("Comment not found.");
        }

        comment.DeletedAt = DateTimeOffset.UtcNow;
        comment.UpdatedBy = userId;
        await repository.SaveCommentAsync(comment, cancellationToken);
    }
}
