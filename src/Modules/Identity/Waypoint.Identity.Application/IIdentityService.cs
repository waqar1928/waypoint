namespace Waypoint.Identity.Application;

public sealed record IdentityOperationResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static IdentityOperationResult Success() => new(true, []);
    public static IdentityOperationResult Failure(IEnumerable<string> errors) => new(false, [.. errors]);
}

public sealed record UserLookup(Guid Id, string Email, bool EmailConfirmed);

/// <summary>Admin-only user listing (Phase 8) — includes lockout state, which UserLookup doesn't expose.</summary>
public sealed record AdminUserLookup(Guid Id, string Email, bool EmailConfirmed, bool IsLockedOut, DateTimeOffset? LockoutEnd);

public enum SignInOutcome
{
    Success,
    InvalidCredentials,
    LockedOut,
    EmailNotConfirmed,
}

public sealed record PasswordSignInResult(SignInOutcome Outcome, Guid? UserId, string? Email);

/// <summary>
/// Application-layer port over identity/credential management. Implemented
/// in Waypoint.Identity.Infrastructure using ASP.NET Core Identity's
/// UserManager/SignInManager — handlers in this project never reference
/// those types directly, so they stay testable without a real database or
/// HTTP context.
/// </summary>
public interface IIdentityService
{
    Task<(IdentityOperationResult Result, Guid? UserId)> CreateUserAsync(
        string email, string password, CancellationToken cancellationToken);

    Task<string> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Returns null if no account exists for the email OR the account is already
    /// confirmed — callers must not leak either distinction to the client (same anti-enumeration
    /// shape as GeneratePasswordResetTokenIfExistsAsync below).</summary>
    Task<(Guid UserId, string Token)?> GenerateEmailConfirmationTokenIfUnconfirmedAsync(
        string email, CancellationToken cancellationToken);

    Task<IdentityOperationResult> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken);

    Task<PasswordSignInResult> PasswordSignInAsync(string email, string password, CancellationToken cancellationToken);

    Task SignOutAsync(CancellationToken cancellationToken);

    /// <summary>Returns null if no account exists for the email — callers must not leak that distinction to the client.</summary>
    Task<(Guid UserId, string Token)?> GeneratePasswordResetTokenIfExistsAsync(string email, CancellationToken cancellationToken);

    Task<IdentityOperationResult> ResetPasswordAsync(
        Guid userId, string token, string newPassword, CancellationToken cancellationToken);

    Task<UserLookup?> FindByIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken);

    Task<IdentityOperationResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Admin-only (Phase 8) — every account, most recently created first isn't tracked
    /// (ApplicationUser has no CreatedAt), so this is ordered by email for stable pagination-free
    /// display. Capped at <paramref name="take"/> as a Phase 10 safety net against unbounded
    /// growth, not a real paging UX — the admin caller passes a generous limit.</summary>
    Task<IReadOnlyList<AdminUserLookup>> GetAllUsersAsync(int take, CancellationToken cancellationToken);

    /// <summary>Locks the account indefinitely (DateTimeOffset.MaxValue) using ASP.NET Core
    /// Identity's existing lockout mechanism — the same one that already locks an account out
    /// after repeated failed logins, just applied deliberately by an admin.</summary>
    Task<IdentityOperationResult> LockUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<IdentityOperationResult> UnlockUserAsync(Guid userId, CancellationToken cancellationToken);
}

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken);
}
