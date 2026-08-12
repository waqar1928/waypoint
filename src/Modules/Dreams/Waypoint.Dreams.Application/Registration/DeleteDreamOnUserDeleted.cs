using MediatR;
using Waypoint.Common;

namespace Waypoint.Dreams.Application.Registration;

/// <summary>
/// Cascade-deletes this module's data when an account is deleted — see
/// docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section, and
/// Waypoint.Users.Application.Registration.DeleteProfileOnUserDeleted for the original instance
/// of this pattern. Dream keys directly off UserId, so this doesn't need the event's DreamId —
/// unlike the modules that only key off DreamId (Goals, Actions, Experiments, BusinessIdeas).
/// </summary>
public sealed class DeleteDreamOnUserDeleted(IDreamRepository repository)
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    public Task Handle(UserDeletedIntegrationEvent notification, CancellationToken cancellationToken) =>
        repository.DeleteForUserAsync(notification.UserId, cancellationToken);
}
