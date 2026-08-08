using MediatR;
using Waypoint.Common;

namespace Waypoint.Users.Application.Registration;

/// <summary>
/// Reacts to the Identity module's UserRegisteredIntegrationEvent by
/// creating this user's Profile/NotificationPreferences/PrivacySettings —
/// the cross-module side effect described in docs/03-domain-model.md
/// "Module communication rules". Identity never writes to Users' tables.
/// </summary>
public sealed class CreateProfileOnUserRegistered(IUsersRepository repository)
    : INotificationHandler<UserRegisteredIntegrationEvent>
{
    public Task Handle(UserRegisteredIntegrationEvent notification, CancellationToken cancellationToken) =>
        repository.CreateDefaultsForNewUserAsync(notification.UserId, notification.DisplayName, cancellationToken);
}
