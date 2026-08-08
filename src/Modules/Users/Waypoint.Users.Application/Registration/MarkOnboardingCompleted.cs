using MediatR;
using Waypoint.Common;

namespace Waypoint.Users.Application.Registration;

/// <summary>Reacts to Dreams publishing OnboardingCompletedIntegrationEvent (first Dream + Dream Statement saved).</summary>
public sealed class MarkOnboardingCompleted(IUsersRepository repository)
    : INotificationHandler<OnboardingCompletedIntegrationEvent>
{
    public async Task Handle(OnboardingCompletedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var profile = await repository.GetProfileAsync(notification.UserId, cancellationToken);
        if (profile is null || profile.OnboardingCompletedAt is not null)
        {
            return;
        }

        profile.OnboardingCompletedAt = DateTimeOffset.UtcNow;
        profile.UpdatedBy = notification.UserId;

        await repository.SaveProfileAsync(profile, cancellationToken);
    }
}
