using Waypoint.Users.Domain;

namespace Waypoint.Users.Application;

/// <summary>
/// Port over persistence for the Users module. One repository covering
/// Profile/NotificationPreferences/PrivacySettings is appropriate here —
/// they're three 1:1-with-user tables that are always read/written together
/// per request; splitting them into three repositories would just be
/// indirection without a testing or reuse benefit.
/// </summary>
public interface IUsersRepository
{
    Task<Profile?> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task SaveProfileAsync(Profile profile, CancellationToken cancellationToken);

    Task<NotificationPreferences?> GetNotificationPreferencesAsync(Guid userId, CancellationToken cancellationToken);
    Task SaveNotificationPreferencesAsync(NotificationPreferences preferences, CancellationToken cancellationToken);

    Task<PrivacySettings?> GetPrivacySettingsAsync(Guid userId, CancellationToken cancellationToken);
    Task SavePrivacySettingsAsync(PrivacySettings settings, CancellationToken cancellationToken);

    Task CreateDefaultsForNewUserAsync(Guid userId, string displayName, CancellationToken cancellationToken);

    Task DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken);
}
