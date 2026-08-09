using Waypoint.Common;

namespace Waypoint.Mentorship.Application;

/// <summary>Shared helper so every handler resolves person display info through
/// IProfileSummaryProvider the same way — mirrors Community.Application's AuthorResolver.</summary>
internal static class PersonResolver
{
    private static PersonDto Fallback(Guid userId) => new(userId, "Waypoint member", null);

    public static async Task<PersonDto> ResolveAsync(
        IProfileSummaryProvider provider, Guid userId, CancellationToken cancellationToken)
    {
        var profile = await provider.GetForUserAsync(userId, cancellationToken);
        return profile is null ? Fallback(userId) : new PersonDto(profile.UserId, profile.DisplayName, profile.AvatarUrl);
    }

    public static async Task<IReadOnlyDictionary<Guid, PersonDto>> ResolveManyAsync(
        IProfileSummaryProvider provider, IReadOnlyList<Guid> userIds, CancellationToken cancellationToken)
    {
        var distinctIds = userIds.Distinct().ToList();
        var profiles = await provider.GetForUsersAsync(distinctIds, cancellationToken);
        return distinctIds.ToDictionary(
            id => id,
            id => profiles.TryGetValue(id, out var p) ? new PersonDto(p.UserId, p.DisplayName, p.AvatarUrl) : Fallback(id));
    }
}
