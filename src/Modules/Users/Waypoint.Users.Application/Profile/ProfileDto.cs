namespace Waypoint.Users.Application.Profiles;

public sealed record ProfileDto(
    Guid UserId,
    string DisplayName,
    string? Bio,
    string? AvatarUrl,
    string TimeZone,
    string Locale,
    DateTimeOffset? OnboardingCompletedAt);
