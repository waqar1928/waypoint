using Waypoint.Common;

namespace Waypoint.Users.Domain;

public sealed class NotificationPreferences : Entity
{
    public Guid UserId { get; init; }
    public bool EmailProductUpdates { get; set; } = true;
    public bool EmailCoachNudges { get; set; } = true;
    public bool EmailCommunityActivity { get; set; }

    public static NotificationPreferences CreateForNewUser(Guid userId) =>
        new() { UserId = userId, CreatedBy = userId, UpdatedBy = userId };
}
