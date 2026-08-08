using Waypoint.Common;

namespace Waypoint.Dreams.Domain;

public sealed class Dream : Entity, ISoftDeletable
{
    public Guid UserId { get; init; }
    public string Title { get; set; } = string.Empty;
    public DreamStage Stage { get; set; } = DreamStage.Discover;
    public bool IsBusinessShaped { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public DreamStatement Statement { get; set; } = null!;

    public static Dream Create(Guid userId, string title, bool isBusinessShaped)
    {
        var dream = new Dream
        {
            UserId = userId,
            Title = title,
            Stage = DreamStage.Define,
            IsBusinessShaped = isBusinessShaped,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
        return dream;
    }
}
