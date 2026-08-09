using Waypoint.Common;

namespace Waypoint.Community.Application;

/// <summary>Shared helper so every handler resolves author display info through
/// IProfileSummaryProvider the same way, with the same graceful fallback if a profile is missing.</summary>
internal static class AuthorResolver
{
    private static AuthorDto Fallback(Guid userId) => new(userId, "Waypoint member", null);

    public static async Task<AuthorDto> ResolveAsync(
        IProfileSummaryProvider provider, Guid userId, CancellationToken cancellationToken)
    {
        var profile = await provider.GetForUserAsync(userId, cancellationToken);
        return profile is null ? Fallback(userId) : new AuthorDto(profile.UserId, profile.DisplayName, profile.AvatarUrl);
    }

    public static async Task<IReadOnlyDictionary<Guid, AuthorDto>> ResolveManyAsync(
        IProfileSummaryProvider provider, IReadOnlyList<Guid> userIds, CancellationToken cancellationToken)
    {
        var distinctIds = userIds.Distinct().ToList();
        var profiles = await provider.GetForUsersAsync(distinctIds, cancellationToken);
        return distinctIds.ToDictionary(
            id => id,
            id => profiles.TryGetValue(id, out var p) ? new AuthorDto(p.UserId, p.DisplayName, p.AvatarUrl) : Fallback(id));
    }
}
