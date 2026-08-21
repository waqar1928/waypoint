using Waypoint.Common;

namespace Waypoint.Users.Domain;

public sealed class NotificationPreferences : Entity
{
    public Guid UserId { get; init; }
    public bool EmailProductUpdates { get; set; } = true;
    public bool EmailCoachNudges { get; set; } = true;
    public bool EmailCommunityActivity { get; set; }

    // Push (P1) — every one of these defaults to "off"/unconfigured. Push is opt-in only; nothing
    // here is ever enabled just by the column existing. See docs on the Settings form for why
    // (Drevia's "calm, not manipulative" product principle).
    public bool PushEnabled { get; set; }
    public bool PushDetailedContent { get; set; }
    public bool PushDailyReminderEnabled { get; set; }

    /// <summary>Null means "not configured" - a distinct state from "configured to zero duration."
    /// Both must be set together for quiet hours to apply; see QuietHoursEvaluator.</summary>
    public TimeOnly? QuietHoursStart { get; set; }
    public TimeOnly? QuietHoursEnd { get; set; }

    public static NotificationPreferences CreateForNewUser(Guid userId) =>
        new() { UserId = userId, CreatedBy = userId, UpdatedBy = userId };
}
