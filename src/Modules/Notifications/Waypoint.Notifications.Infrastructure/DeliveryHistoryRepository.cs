using Microsoft.EntityFrameworkCore;
using Waypoint.Notifications.Application.Push;
using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Infrastructure;

public sealed class DeliveryHistoryRepository(NotificationsDbContext db) : IDeliveryHistoryRepository
{
    public Task<NotificationDeliveryHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.DeliveryHistory.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<int> CountSentInWindowAsync(
        Guid userId, DateTimeOffset localDayStartUtc, DateTimeOffset localDayEndUtc, CancellationToken cancellationToken) =>
        db.DeliveryHistory.CountAsync(
            d => d.UserId == userId && d.Status == DeliveryStatus.Sent
                 && d.AttemptedAt >= localDayStartUtc && d.AttemptedAt < localDayEndUtc,
            cancellationToken);

    public Task MarkSkippedAsync(Guid id, string reason, CancellationToken cancellationToken) =>
        db.DeliveryHistory
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.Status, DeliveryStatus.Skipped)
                .SetProperty(d => d.FailureReason, reason)
                .SetProperty(d => d.CompletedAt, DateTimeOffset.UtcNow), cancellationToken);

    public Task MarkSentAsync(Guid id, CancellationToken cancellationToken) =>
        db.DeliveryHistory
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.Status, DeliveryStatus.Sent)
                .SetProperty(d => d.CompletedAt, DateTimeOffset.UtcNow), cancellationToken);

    public Task MarkFailedAsync(Guid id, string reason, CancellationToken cancellationToken) =>
        db.DeliveryHistory
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.Status, DeliveryStatus.Failed)
                .SetProperty(d => d.FailureReason, reason)
                .SetProperty(d => d.CompletedAt, DateTimeOffset.UtcNow), cancellationToken);

    public Task MarkAttemptedAgainAsync(Guid id, CancellationToken cancellationToken) =>
        db.DeliveryHistory
            .Where(d => d.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(d => d.AttemptedAt, DateTimeOffset.UtcNow), cancellationToken);
}
