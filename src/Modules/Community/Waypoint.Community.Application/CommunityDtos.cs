using Waypoint.Community.Domain;

namespace Waypoint.Community.Application;

public sealed record AuthorDto(Guid UserId, string DisplayName, string? AvatarUrl);

/// <summary>
/// Deliberately lean - title and statement only, not the full DreamSummary's purpose/who-it-
/// helps/problem/outcome/motivation/impact fields. Attaching a Dream to a post is opt-in (see
/// CreatePostCommand's AttachDream flag), but even opted in, a stranger in the feed doesn't need
/// the same depth of detail Drevia Coach gets - this is what's shown to other people, not what's
/// stored privately.
/// </summary>
public sealed record AttachedDreamDto(string Title, string Statement);

public sealed record PostDto(
    Guid Id,
    AuthorDto Author,
    string Body,
    PostVisibility Visibility,
    AttachedDreamDto? AttachedDream,
    int CommentCount,
    bool IsMine,
    DateTimeOffset CreatedAt)
{
    public static PostDto From(
        CommunityPost post, AuthorDto author, AttachedDreamDto? attachedDream, int commentCount, Guid currentUserId) => new(
        post.Id, author, post.Body, post.Visibility, attachedDream, commentCount, post.UserId == currentUserId, post.CreatedAt);
}

public sealed record CommentDto(Guid Id, AuthorDto Author, string Body, bool IsMine, DateTimeOffset CreatedAt)
{
    public static CommentDto From(Comment comment, AuthorDto author, Guid currentUserId) => new(
        comment.Id, author, comment.Body, comment.UserId == currentUserId, comment.CreatedAt);
}
