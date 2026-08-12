using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Waypoint.Notifications.Application.GetMyNotifications;
using Waypoint.Notifications.Application.GetUnreadCount;
using Waypoint.Notifications.Application.MarkAllAsRead;
using Waypoint.Notifications.Application.MarkAsRead;

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

        return app;
    }
}
