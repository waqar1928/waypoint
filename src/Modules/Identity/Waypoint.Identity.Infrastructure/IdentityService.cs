using Microsoft.AspNetCore.Identity;
using Waypoint.Identity.Application;

namespace Waypoint.Identity.Infrastructure;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager)
    : IIdentityService
{
    public async Task<(IdentityOperationResult Result, Guid? UserId)> CreateUserAsync(
        string email, string password, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user, password);

        return result.Succeeded
            ? (IdentityOperationResult.Success(), user.Id)
            : (IdentityOperationResult.Failure(result.Errors.Select(e => e.Description)), null);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found.");
        return await userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<IdentityOperationResult> ConfirmEmailAsync(
        Guid userId, string token, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return IdentityOperationResult.Failure(["User not found."]);
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<PasswordSignInResult> PasswordSignInAsync(
        string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return new PasswordSignInResult(SignInOutcome.InvalidCredentials, null, null);
        }

        var result = await signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return new PasswordSignInResult(SignInOutcome.Success, user.Id, user.Email);
        }

        return result.IsLockedOut
            ? new PasswordSignInResult(SignInOutcome.LockedOut, null, null)
            : new PasswordSignInResult(SignInOutcome.InvalidCredentials, null, null);
    }

    public Task SignOutAsync(CancellationToken cancellationToken) => signInManager.SignOutAsync();

    public async Task<(Guid UserId, string Token)?> GeneratePasswordResetTokenIfExistsAsync(
        string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return (user.Id, token);
    }

    public async Task<IdentityOperationResult> ResetPasswordAsync(
        Guid userId, string token, string newPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return IdentityOperationResult.Failure(["User not found."]);
        }

        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<UserLookup?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : new UserLookup(user.Id, user.Email!, user.EmailConfirmed);
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is not null && await userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IdentityOperationResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return IdentityOperationResult.Failure(["User not found."]);
        }

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure(result.Errors.Select(e => e.Description));
    }
}
