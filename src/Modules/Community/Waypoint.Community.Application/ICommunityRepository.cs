using Waypoint.Community.Domain;

namespace Waypoint.Community.Application;

public interface ICommunityRepository
{
    Task<CommunityPost?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CommunityPost>> GetPostsForUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Every non-Private post from every user — Public and Community currently behave
    /// identically since there's no external/unauthenticated surface yet (see PostVisibility).
    /// Most-recent-first, capped at <paramref name="take"/> — this is a global, cross-user feed
    /// with no natural per-user bound, so it needs a safety cap as the user base grows.</summary>
    Task<IReadOnlyList<CommunityPost>> GetFeedAsync(int take, CancellationToken cancellationToken);

    Task AddPostAsync(CommunityPost post, CancellationToken cancellationToken);
    Task SavePostAsync(CommunityPost post, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, int>> GetCommentCountsAsync(IReadOnlyList<Guid> postIds, CancellationToken cancellationToken);

    Task<Comment?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Comment>> GetCommentsForPostAsync(Guid postId, CancellationToken cancellationToken);
    Task AddCommentAsync(Comment comment, CancellationToken cancellationToken);
    Task SaveCommentAsync(Comment comment, CancellationToken cancellationToken);

    /// <summary>Batched by Id — avoids issuing one query per report when resolving moderation-queue previews.</summary>
    Task<IReadOnlyDictionary<Guid, CommunityPost>> GetPostsByIdsAsync(IReadOnlyList<Guid> postIds, CancellationToken cancellationToken);

    /// <summary>Batched by Id — avoids issuing one query per report when resolving moderation-queue previews.</summary>
    Task<IReadOnlyDictionary<Guid, Comment>> GetCommentsByIdsAsync(IReadOnlyList<Guid> commentIds, CancellationToken cancellationToken);

    Task<IReadOnlyList<ContentReportRecord>> GetOpenReportsAsync(CancellationToken cancellationToken);
    Task<ContentReportRecord?> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken);
    Task SaveReportAsync(ContentReportRecord report, CancellationToken cancellationToken);
}
