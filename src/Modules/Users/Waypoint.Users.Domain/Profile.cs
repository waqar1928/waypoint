using Waypoint.Common;

namespace Waypoint.Users.Domain;

public sealed class Profile : Entity
{
    public Guid UserId { get; init; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string Locale { get; set; } = "en-US";
    public DateTimeOffset? OnboardingCompletedAt { get; set; }

    public static Profile CreateForNewUser(Guid userId, string displayName) =>
        new()
        {
            UserId = userId,
            DisplayName = displayName,
            CreatedBy = userId,
            UpdatedBy = userId,
        };
}
