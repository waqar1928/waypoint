using Microsoft.EntityFrameworkCore;
using Waypoint.Users.Application;
using Waypoint.Users.Domain;

namespace Waypoint.Users.Infrastructure;

public sealed class UsersRepository(UsersDbContext db) : IUsersRepository
{
    public Task<Profile?> GetProfileAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Profiles.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public async Task SaveProfileAsync(Profile profile, CancellationToken cancellationToken)
    {
        if (db.Entry(profile).State == EntityState.Detached)
        {
            db.Profiles.Update(profile);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<NotificationPreferences?> GetNotificationPreferencesAsync(Guid userId, CancellationToken cancellationToken) =>
        db.NotificationPreferences.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public async Task SaveNotificationPreferencesAsync(NotificationPreferences preferences, CancellationToken cancellationToken)
    {
        if (db.Entry(preferences).State == EntityState.Detached)
        {
            db.NotificationPreferences.Update(preferences);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<PrivacySettings?> GetPrivacySettingsAsync(Guid userId, CancellationToken cancellationToken) =>
        db.PrivacySettings.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    public async Task SavePrivacySettingsAsync(PrivacySettings settings, CancellationToken cancellationToken)
    {
        if (db.Entry(settings).State == EntityState.Detached)
        {
            db.PrivacySettings.Update(settings);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateDefaultsForNewUserAsync(Guid userId, string displayName, CancellationToken cancellationToken)
    {
        db.Profiles.Add(Profile.CreateForNewUser(userId, displayName));
        db.NotificationPreferences.Add(NotificationPreferences.CreateForNewUser(userId));
        db.PrivacySettings.Add(PrivacySettings.CreateForNewUser(userId));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        await db.Profiles.Where(p => p.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.NotificationPreferences.Where(p => p.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.PrivacySettings.Where(p => p.UserId == userId).ExecuteDeleteAsync(cancellationToken);
    }
}
