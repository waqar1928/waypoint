namespace Waypoint.Identity.Application.Admin;

public sealed record AdminUserDto(
    Guid Id,
    string Email,
    string? DisplayName,
    bool EmailConfirmed,
    bool IsLockedOut,
    DateTimeOffset? LockoutEnd);
