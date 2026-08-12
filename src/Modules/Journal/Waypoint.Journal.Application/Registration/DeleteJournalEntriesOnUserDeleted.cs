using MediatR;
using Waypoint.Common;

namespace Waypoint.Journal.Application.Registration;

/// <summary>
/// Cascade-deletes this module's data when an account is deleted — see
/// docs/PRODUCTION_READINESS_AUDIT.md's Data Protection section, and
/// Waypoint.Users.Application.Registration.DeleteProfileOnUserDeleted for the original instance
/// of this pattern. JournalEntry keys directly off UserId, so this doesn't need the event's
/// DreamId.
/// </summary>
public sealed class DeleteJournalEntriesOnUserDeleted(IJournalRepository repository)
    : INotificationHandler<UserDeletedIntegrationEvent>
{
    public Task Handle(UserDeletedIntegrationEvent notification, CancellationToken cancellationToken) =>
        repository.DeleteAllForUserAsync(notification.UserId, cancellationToken);
}
