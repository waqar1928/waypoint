using Waypoint.Common;

namespace Waypoint.Users.Domain;

public enum VisibilityLevel
{
    Private,
    Followers,
    Community,
    Public,
}

public sealed class PrivacySettings : Entity
{
    public Guid UserId { get; init; }
    public VisibilityLevel ProfileVisibility { get; set; } = VisibilityLevel.Private;
    public VisibilityLevel DreamVisibility { get; set; } = VisibilityLevel.Private;

    public static PrivacySettings CreateForNewUser(Guid userId) =>
        new() { UserId = userId, CreatedBy = userId, UpdatedBy = userId };
}
