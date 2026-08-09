using Waypoint.Common;

namespace Waypoint.Community.Domain;

public sealed class Comment : Entity, ISoftDeletable
{
    public Guid PostId { get; init; }
    public Guid UserId { get; init; }
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset? DeletedAt { get; set; }

    public static Comment Create(Guid postId, Guid userId, string body) =>
        new()
        {
            PostId = postId,
            UserId = userId,
            Body = body,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
