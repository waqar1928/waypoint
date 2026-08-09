using Microsoft.EntityFrameworkCore;
using Waypoint.Common;

namespace Waypoint.Users.Infrastructure;

/// <summary>Implements the cross-module IProfileSummaryProvider read contract — see docs/03-domain-model.md.</summary>
public sealed class ProfileSummaryProvider(UsersDbContext db) : IProfileSummaryProvider
{
    public async Task<ProfileSummary?> GetForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await db.Profiles.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        return profile is null ? null : new ProfileSummary(profile.UserId, profile.DisplayName, profile.AvatarUrl);
    }

    public async Task<IReadOnlyDictionary<Guid, ProfileSummary>> GetForUsersAsync(
        IReadOnlyList<Guid> userIds, CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, ProfileSummary>();
        }

        var profiles = await db.Profiles
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync(cancellationToken);

        return profiles.ToDictionary(p => p.UserId, p => new ProfileSummary(p.UserId, p.DisplayName, p.AvatarUrl));
    }
}
