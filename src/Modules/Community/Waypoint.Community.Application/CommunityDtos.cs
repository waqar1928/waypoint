using Waypoint.Community.Domain;

namespace Waypoint.Community.Application;

public sealed record AuthorDto(Guid UserId, string DisplayName, string? AvatarUrl);

public sealed record PostDto(
    Guid Id,
    AuthorDto Author,
    string Body,
    PostVisibility Visibility,
    int CommentCount,
    bool IsMine,
    DateTimeOffset CreatedAt)
{
    public static PostDto From(CommunityPost post, AuthorDto author, int commentCount, Guid currentUserId) => new(
        post.Id, author, post.Body, post.Visibility, commentCount, post.UserId == currentUserId, post.CreatedAt);
}

public sealed record CommentDto(Guid Id, AuthorDto Author, string Body, bool IsMine, DateTimeOffset CreatedAt)
{
    public static CommentDto From(Comment comment, AuthorDto author, Guid currentUserId) => new(
        comment.Id, author, comment.Body, comment.UserId == currentUserId, comment.CreatedAt);
}
