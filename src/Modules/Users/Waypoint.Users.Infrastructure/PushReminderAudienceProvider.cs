using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Users.Infrastructure;

/// <summary>Implements the cross-module IPushReminderAudienceProvider read contract — see
/// Waypoint.Common/PushNotifications.cs. One query joining Profile (for TimeZone, the single
/// source of truth - never duplicated onto NotificationPreferences) and NotificationPreferences
/// (for the push opt-ins/quiet hours), since both are Users-owned 1:1-with-user tables in this
/// same DbContext.</summary>
public sealed class PushReminderAudienceProvider(UsersDbContext db) : IPushReminderAudienceProvider
{
    public async Task<IReadOnlyList<PushReminderCandidate>> GetDailyReminderCandidatesAsync(
        CancellationToken cancellationToken)
    {
        return await (
            from prefs in db.NotificationPreferences
            join profile in db.Profiles on prefs.UserId equals profile.UserId
            where prefs.PushEnabled && prefs.PushDailyReminderEnabled
            select new PushReminderCandidate(
                prefs.UserId,
                profile.TimeZone,
                prefs.QuietHoursStart,
                prefs.QuietHoursEnd,
                prefs.PushDetailedContent))
            .ToListAsync(cancellationToken);
    }
}
