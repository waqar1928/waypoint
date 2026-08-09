using Waypoint.Community.Domain;

namespace Waypoint.Community.Application;

public interface ICommunityRepository
{
    Task<CommunityPost?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CommunityPost>> GetPostsForUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>Every non-Private post from every user — Public and Community currently behave
    /// identically since there's no external/unauthenticated surface yet (see PostVisibility).</summary>
    Task<IReadOnlyList<CommunityPost>> GetFeedAsync(CancellationToken cancellationToken);

    Task AddPostAsync(CommunityPost post, CancellationToken cancellationToken);
    Task SavePostAsync(CommunityPost post, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, int>> GetCommentCountsAsync(IReadOnlyList<Guid> postIds, CancellationToken cancellationToken);

    Task<Comment?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Comment>> GetCommentsForPostAsync(Guid postId, CancellationToken cancellationToken);
    Task AddCommentAsync(Comment comment, CancellationToken cancellationToken);
    Task SaveCommentAsync(Comment comment, CancellationToken cancellationToken);

    Task<IReadOnlyList<ContentReportRecord>> GetOpenReportsAsync(CancellationToken cancellationToken);
    Task<ContentReportRecord?> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken);
    Task SaveReportAsync(ContentReportRecord report, CancellationToken cancellationToken);
}
