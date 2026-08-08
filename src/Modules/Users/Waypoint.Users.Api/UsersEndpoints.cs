using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Waypoint.Users.Application.Preferences;
using Waypoint.Users.Application.Privacy;
using Waypoint.Users.Application.Profiles;

namespace Waypoint.Users.Api;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/me").RequireAuthorization().RequireRateLimiting("api").WithTags("Me");

        group.MapGet("/profile", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMyProfileQuery(), ct)));

        group.MapPut("/profile", async (UpdateMyProfileCommand command, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(command, ct)));

        group.MapGet("/notification-preferences", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetNotificationPreferencesQuery(), ct)));

        group.MapPut("/notification-preferences",
            async (UpdateNotificationPreferencesCommand command, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(command, ct)));

        group.MapGet("/privacy-settings", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetPrivacySettingsQuery(), ct)));

        group.MapPut("/privacy-settings",
            async (UpdatePrivacySettingsCommand command, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(command, ct)));

        return app;
    }
}
