using FluentValidation;
using MediatR;
using Waypoint.Common;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Application.CreateComment;

public sealed record CreateCommentCommand(Guid PostId, string Body) : IRequest<CommentDto>;

public sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.PostId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(1000);
    }
}

public sealed class CreateCommentCommandHandler(
    ICommunityRepository repository,
    IProfileSummaryProvider profileSummaryProvider,
    ICurrentUserAccessor currentUser,
    INotificationSink notificationSink)
    : IRequestHandler<CreateCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new AuthenticationFailedException("Not signed in.");
        var post = await repository.GetPostByIdAsync(request.PostId, cancellationToken)
            ?? throw new NotFoundException("Post not found.");

        // Only the owner can comment on their own Private post; Community/Public posts are open.
        if (post.Visibility == PostVisibility.Private && post.UserId != userId)
        {
            throw new NotFoundException("Post not found.");
        }

        var comment = Comment.Create(request.PostId, userId, request.Body);
        await repository.AddCommentAsync(comment, cancellationToken);

        // Notify the post's author, but never notify someone about their own comment on their own
        // post — a real, low-frequency, high-signal event worth surfacing (see
        // docs/PRODUCTION_READINESS_AUDIT.md's Notifications module writeup).
        if (post.UserId != userId)
        {
            await notificationSink.SendAsync(
                new NotificationToSend(
                    post.UserId,
                    NotificationCategories.CommunityActivity,
                    "New comment on your post",
                    request.Body.Length > 140 ? request.Body[..140] + "…" : request.Body,
                    "/app/community",
                    DateTimeOffset.UtcNow),
                cancellationToken);
        }

        var author = await AuthorResolver.ResolveAsync(profileSummaryProvider, userId, cancellationToken);
        return CommentDto.From(comment, author, userId);
    }
}
