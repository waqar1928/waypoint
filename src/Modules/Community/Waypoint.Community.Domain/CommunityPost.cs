using Waypoint.Common;

namespace Waypoint.Community.Domain;

public sealed class CommunityPost : Entity, ISoftDeletable
{
    public Guid UserId { get; init; }
    public Guid? DreamId { get; init; }
    public string Body { get; set; } = string.Empty;
    public PostVisibility Visibility { get; set; } = PostVisibility.Private;
    public DateTimeOffset? DeletedAt { get; set; }

    public static CommunityPost Create(Guid userId, Guid? dreamId, string body, PostVisibility visibility) =>
        new()
        {
            UserId = userId,
            DreamId = dreamId,
            Body = body,
            Visibility = visibility,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
