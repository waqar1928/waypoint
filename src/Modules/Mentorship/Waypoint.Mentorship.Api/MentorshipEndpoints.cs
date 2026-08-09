using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Waypoint.Mentorship.Application.CloseHelpRequest;
using Waypoint.Mentorship.Application.CreateHelpRequest;
using Waypoint.Mentorship.Application.CreateMentorProfile;
using Waypoint.Mentorship.Application.GetHelpRequestResponses;
using Waypoint.Mentorship.Application.GetHelpRequests;
using Waypoint.Mentorship.Application.GetMentorDirectory;
using Waypoint.Mentorship.Application.GetMyHelpRequests;
using Waypoint.Mentorship.Application.GetMyMentorProfile;
using Waypoint.Mentorship.Application.RespondToHelpRequest;
using Waypoint.Mentorship.Application.UpdateMentorVerification;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Api;

public static class MentorshipEndpoints
{
    public static IEndpointRouteBuilder MapMentorshipEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/mentorship").RequireAuthorization().RequireRateLimiting("api").WithTags("Mentorship");

        group.MapGet("/mentors", async (string? expertise, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMentorDirectoryQuery(expertise), ct)));

        group.MapGet("/mentors/me", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetMyMentorProfileQuery(), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapPut("/mentors/me", async (CreateMentorProfileRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new CreateMentorProfileCommand(body.Expertise, body.YearsExperience, body.Availability), ct)));

        group.MapGet("/help-requests", async (HelpRequestCategory? category, HelpRequestStatus? status, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetHelpRequestsQuery(category, status), ct)));

        group.MapGet("/help-requests/mine", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMyHelpRequestsQuery(), ct)));

        group.MapPost("/help-requests", async (CreateHelpRequestRequest body, ISender sender, CancellationToken ct) =>
            Results.Created("/api/v1/mentorship/help-requests", await sender.Send(
                new CreateHelpRequestCommand(body.Category, body.Title, body.Body, body.DreamId), ct)));

        group.MapGet("/help-requests/{helpRequestId:guid}/responses", async (Guid helpRequestId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetHelpRequestResponsesQuery(helpRequestId), ct)));

        group.MapPost("/help-requests/{helpRequestId:guid}/responses", async (Guid helpRequestId, RespondRequest body, ISender sender, CancellationToken ct) =>
            Results.Created($"/api/v1/mentorship/help-requests/{helpRequestId}/responses", await sender.Send(
                new RespondToHelpRequestCommand(helpRequestId, body.Body), ct)));

        group.MapPost("/help-requests/{helpRequestId:guid}/close", async (Guid helpRequestId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new CloseHelpRequestCommand(helpRequestId), ct)));

        var admin = app.MapGroup("/api/v1/admin/mentors")
            .RequireAuthorization("Admin")
            .RequireRateLimiting("api")
            .WithTags("Admin");

        admin.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMentorDirectoryQuery(null), ct)));

        admin.MapPut("/{mentorProfileId:guid}/verification", async (Guid mentorProfileId, UpdateVerificationRequest body, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new UpdateMentorVerificationCommand(mentorProfileId, body.Status), ct)));

        return app;
    }
}

public sealed record CreateMentorProfileRequest(List<string> Expertise, int? YearsExperience, string? Availability);
public sealed record CreateHelpRequestRequest(HelpRequestCategory Category, string Title, string Body, Guid? DreamId);
public sealed record RespondRequest(string Body);
public sealed record UpdateVerificationRequest(VerificationStatus Status);
