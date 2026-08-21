using Waypoint.Notifications.Domain;

namespace Waypoint.Notifications.Application.Push;

public interface IPushSender
{
    Task SendAsync(PushSubscription subscription, PushPayload payload, CancellationToken cancellationToken);
}

/// <summary>Distinguishes a permanent failure (404/410 Gone - the push service's explicit "this
/// will never work again" signal, per the Web Push spec) from a transient one (timeout/5xx) - see
/// PushSubscriptionLifecycle.ShouldDeactivateAfterFailure, which branches on IsPermanent. Lives in
/// the Application layer (not Infrastructure, where the concrete WebPushSender/RFC 8291
/// implementation lives) so ReminderDeliveryProcessor can be unit tested against a mocked
/// IPushSender without depending on the WebPush package at all.</summary>
public sealed class PushDeliveryException(bool isPermanent, int? statusCode, string message) : Exception(message)
{
    public bool IsPermanent { get; } = isPermanent;
    public int? StatusCode { get; } = statusCode;
}
