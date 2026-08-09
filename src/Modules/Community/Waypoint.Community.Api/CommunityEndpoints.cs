using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Waypoint.Community.Application.CreateComment;
using Waypoint.Community.Application.CreatePost;
using Waypoint.Community.Application.DeleteComment;
using Waypoint.Community.Application.DeletePost;
using Waypoint.Community.Application.DismissReport;
using Waypoint.Community.Application.GetCommunityFeed;
using Waypoint.Community.Application.GetModerationQueue;
using Waypoint.Community.Application.GetMyPosts;
using Waypoint.Community.Application.GetPostComments;
using Waypoint.Community.Application.RemoveReportedContent;
using Waypoint.Community.Application.ReportContent;
using Waypoint.Community.Application.ResolveReport;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Api;

public static class CommunityEndpoints
{
    public static IEndpointRouteBuilder MapCommunityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/community").RequireAuthorization().RequireRateLimiting("api").WithTags("Community");

        group.MapGet("/feed", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetCommunityFeedQuery(), ct)));

        group.MapGet("/posts/mine", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetMyPostsQuery(), ct)));

        group.MapPost("/posts", async (CreatePostRequest body, ISender sender, CancellationToken ct) =>
            Results.Created("/api/v1/community/feed", await sender.Send(new CreatePostCommand(body.Body, body.Visibility, body.DreamId), ct)));

        group.MapDelete("/posts/{postId:guid}", async (Guid postId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeletePostCommand(postId), ct);
            return Results.NoContent();
        });

        group.MapGet("/posts/{postId:guid}/comments", async (Guid postId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetPostCommentsQuery(postId), ct)));

        group.MapPost("/posts/{postId:guid}/comments", async (Guid postId, CreateCommentRequest body, ISender sender, CancellationToken ct) =>
            Results.Created($"/api/v1/community/posts/{postId}/comments", await sender.Send(new CreateCommentCommand(postId, body.Body), ct)));

        group.MapDelete("/comments/{commentId:guid}", async (Guid commentId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteCommentCommand(commentId), ct);
            return Results.NoContent();
        });

        group.MapPost("/reports", async (ReportContentRequest body, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ReportContentCommand(body.EntityType, body.EntityId, body.Reason, body.Details), ct);
            return Results.NoContent();
        });

        var admin = app.MapGroup("/api/v1/admin/moderation")
            .RequireAuthorization("Admin")
            .RequireRateLimiting("api")
            .WithTags("Admin");

        admin.MapGet("/", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetModerationQueueQuery(), ct)));

        admin.MapPost("/{reportId:guid}/dismiss", async (Guid reportId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DismissReportCommand(reportId), ct);
            return Results.NoContent();
        });

        admin.MapPost("/{reportId:guid}/remove-content", async (Guid reportId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new RemoveReportedContentCommand(reportId), ct);
            return Results.NoContent();
        });

        admin.MapPost("/{reportId:guid}/resolve", async (Guid reportId, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new ResolveReportCommand(reportId), ct);
            return Results.NoContent();
        });

        return app;
    }
}

public sealed record CreatePostRequest(string Body, PostVisibility Visibility, Guid? DreamId);
public sealed record CreateCommentRequest(string Body);
public sealed record ReportContentRequest(string EntityType, Guid EntityId, string Reason, string? Details);
