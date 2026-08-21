using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Waypoint.Notifications.Application.GetMyNotifications;
using Waypoint.Notifications.Application.GetUnreadCount;
using Waypoint.Notifications.Application.MarkAllAsRead;
using Waypoint.Notifications.Application.MarkAsRead;
using Waypoint.Notifications.Application.Push;

namespace Waypoint.Notifications.Api;

public static class NotificationsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("Notifications");

        group.MapGet("/", async (int? take, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMyNotificationsQuery(take ?? 50), ct)));

        group.MapGet("/unread-count", async (ISender sender, CancellationToken ct) =>
            Results.Ok(new { count = await sender.Send(new GetUnreadCountQuery(), ct) }));

        group.MapPost("/{notificationId:guid}/read", async (Guid notificationId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new MarkNotificationReadCommand(notificationId), ct);
            return Results.NoContent();
        });

        group.MapPost("/read-all", async (ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new MarkAllNotificationsReadCommand(), ct);
            return Results.NoContent();
        });

        // The VAPID public key is meant to reach the browser (it's embedded in
        // pushManager.subscribe({applicationServerKey: ...})) - not a secret, so no special
        // protection beyond the group's normal auth. Returns 404 rather than an empty string when
        // push isn't configured, so the frontend's feature-detection has a clean signal to react
        // to instead of silently trying to subscribe with a blank key.
        group.MapGet("/push-public-key", (IConfiguration configuration) =>
        {
            var publicKey = configuration["Waypoint:Notifications:Push:VapidPublicKey"];
            return string.IsNullOrWhiteSpace(publicKey)
                ? Results.NotFound()
                : Results.Ok(new { publicKey });
        });

        group.MapGet("/push-subscriptions", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMyPushSubscriptionsQuery(), ct)));

        group.MapPost("/push-subscriptions", async (SubscribePushRequest request, ISender sender, CancellationToken ct) =>
        {
            // request.Keys can arrive as null despite the non-nullable annotation - System.Text.Json
            // doesn't enforce constructor-parameter nullability at deserialization time by default,
            // so a malformed request body (e.g. missing "keys" entirely) must be handled here rather
            // than assumed away, or it throws a raw NullReferenceException before
            // SubscribeToPushCommandValidator ever gets a chance to return a clean 400. Falling back
            // to "" lets the existing NotEmpty() rules on P256dh/Auth catch it properly instead.
            var result = await sender.Send(
                new SubscribeToPushCommand(
                    request.Endpoint, request.Keys?.P256dh ?? string.Empty, request.Keys?.Auth ?? string.Empty, request.UserAgent),
                ct);
            return Results.Ok(result);
        });

        group.MapDelete("/push-subscriptions/{subscriptionId:guid}", async (Guid subscriptionId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new UnsubscribeFromPushCommand(subscriptionId), ct);
            return Results.NoContent();
        });

        return app;
    }
}

/// <summary>Mirrors the shape of the browser's own PushSubscriptionJSON - see
/// apps/web/src/lib/push-notifications.ts's subscribeToPush(). Keys is genuinely nullable (not
/// just annotated that way defensively): System.Text.Json doesn't enforce non-null constructor
/// parameters at deserialization time, so a client can omit "keys" entirely and this will
/// deserialize with Keys = null rather than throwing - see the endpoint handler above for how
/// that's handled safely.</summary>
public sealed record SubscribePushRequest(string Endpoint, SubscribePushKeys? Keys, string? UserAgent);

public sealed record SubscribePushKeys(string P256dh, string Auth);
