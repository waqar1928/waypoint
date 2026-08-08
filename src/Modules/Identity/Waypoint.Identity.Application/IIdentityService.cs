namespace Waypoint.Identity.Application;

public sealed record IdentityOperationResult(bool Succeeded, IReadOnlyList<string> Errors)
{
    public static IdentityOperationResult Success() => new(true, []);
    public static IdentityOperationResult Failure(IEnumerable<string> errors) => new(false, [.. errors]);
}

public sealed record UserLookup(Guid Id, string Email, bool EmailConfirmed);

public enum SignInOutcome
{
    Success,
    InvalidCredentials,
    LockedOut,
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
}

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken);
}
