namespace Waypoint.Identity.Application;

/// <summary>
/// Read contract for onboarding status, owned and implemented by the Users
/// module (see docs/03-domain-model.md "Module communication rules").
/// Identity depends only on this interface, never on Users' DbContext or
/// Application layer directly.
/// </summary>
public interface IOnboardingStatusProvider
{
    Task<bool> HasCompletedOnboardingAsync(Guid userId, CancellationToken cancellationToken);
}
