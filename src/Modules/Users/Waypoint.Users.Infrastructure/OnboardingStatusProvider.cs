using Microsoft.EntityFrameworkCore;
using Waypoint.Identity.Application;

namespace Waypoint.Users.Infrastructure;

/// <summary>Implements Identity's IOnboardingStatusProvider read contract — see docs/03-domain-model.md.</summary>
public sealed class OnboardingStatusProvider(UsersDbContext db) : IOnboardingStatusProvider
{
    public Task<bool> HasCompletedOnboardingAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Profiles.AnyAsync(p => p.UserId == userId && p.OnboardingCompletedAt != null, cancellationToken);
}
