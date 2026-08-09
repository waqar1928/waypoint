using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Waypoint.Identity.Application.Admin.GetAllUsers;
using Waypoint.Identity.Application.Admin.LockUser;
using Waypoint.Identity.Application.Admin.UnlockUser;
using Waypoint.Identity.Application.DeleteAccount;
using Waypoint.Identity.Application.ForgotPassword;
using Waypoint.Identity.Application.Login;
using Waypoint.Identity.Application.Logout;
using Waypoint.Identity.Application.Register;
using Waypoint.Identity.Application.ResetPassword;
using Waypoint.Identity.Application.Session;
using Waypoint.Identity.Application.VerifyEmail;

namespace Waypoint.Identity.Api;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth").RequireRateLimiting("auth");

        group.MapPost("/register", async (RegisterUserCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/v1/me/profile", result);
        });

        group.MapPost("/login", async (LoginCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Ok(result);
        });

        group.MapPost("/logout", async (ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new LogoutCommand(), ct);
            return Results.NoContent();
        });

        group.MapGet("/session", async (ISender sender, CancellationToken ct) =>
        {
            var session = await sender.Send(new GetSessionQuery(), ct);
            return session is null ? Results.Unauthorized() : Results.Ok(session);
        });

        group.MapPost("/verify-email", async (VerifyEmailCommand command, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(command, ct);
            return Results.Ok();
        });

        group.MapPost("/forgot-password", async (ForgotPasswordCommand command, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(command, ct);
            return Results.Accepted();
        });

        group.MapPost("/reset-password", async (ResetPasswordCommand command, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(command, ct);
            return Results.Ok();
        });

        app.MapDelete("/api/v1/me", async ([FromBody] DeleteAccountRequest body, ISender sender, CancellationToken ct) =>
            {
                await sender.Send(new DeleteAccountCommand(body.Password), ct);
                return Results.Accepted();
            })
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithTags("Account");

        var admin = app.MapGroup("/api/v1/admin/users")
            .RequireAuthorization("Admin")
            .RequireRateLimiting("api")
            .WithTags("Admin");

        admin.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetAllUsersQuery(), ct)));

        admin.MapPost("/{userId:guid}/lock", async (Guid userId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new LockUserCommand(userId), ct);
            return Results.NoContent();
        });

        admin.MapPost("/{userId:guid}/unlock", async (Guid userId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new UnlockUserCommand(userId), ct);
            return Results.NoContent();
        });

        return app;
    }
}

public sealed record DeleteAccountRequest(string Password);
