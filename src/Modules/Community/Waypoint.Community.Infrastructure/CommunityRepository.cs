using Microsoft.EntityFrameworkCore;
using Waypoint.Community.Application;
using Waypoint.Community.Domain;

namespace Waypoint.Community.Infrastructure;

public sealed class CommunityRepository(CommunityDbContext db) : ICommunityRepository
{
    public Task<CommunityPost?> GetPostByIdAsync(Guid postId, CancellationToken cancellationToken) =>
        db.Posts.SingleOrDefaultAsync(p => p.Id == postId && p.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<CommunityPost>> GetPostsForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.Posts.AsNoTracking().Where(p => p.UserId == userId && p.DeletedAt == null).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CommunityPost>> GetFeedAsync(int take, CancellationToken cancellationToken) =>
        await db.Posts
            .AsNoTracking()
            .Where(p => p.Visibility != PostVisibility.Private && p.DeletedAt == null)
            .OrderByDescending(p => p.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task AddPostAsync(CommunityPost post, CancellationToken cancellationToken)
    {
        db.Posts.Add(post);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SavePostAsync(CommunityPost post, CancellationToken cancellationToken)
    {
        if (db.Entry(post).State == EntityState.Detached)
        {
            db.Posts.Update(post);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetCommentCountsAsync(
        IReadOnlyList<Guid> postIds, CancellationToken cancellationToken)
    {
        if (postIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        return await db.Comments
            .Where(c => postIds.Contains(c.PostId) && c.DeletedAt == null)
            .GroupBy(c => c.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count, cancellationToken);
    }

    public Task<Comment?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken) =>
        db.Comments.SingleOrDefaultAsync(c => c.Id == commentId && c.DeletedAt == null, cancellationToken);

    public async Task<IReadOnlyList<Comment>> GetCommentsForPostAsync(Guid postId, CancellationToken cancellationToken) =>
        await db.Comments
            .AsNoTracking()
            .Where(c => c.PostId == postId && c.DeletedAt == null)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, CommunityPost>> GetPostsByIdsAsync(
        IReadOnlyList<Guid> postIds, CancellationToken cancellationToken)
    {
        if (postIds.Count == 0)
        {
            return new Dictionary<Guid, CommunityPost>();
        }

        return await db.Posts
            .AsNoTracking()
            .Where(p => postIds.Contains(p.Id) && p.DeletedAt == null)
            .ToDictionaryAsync(p => p.Id, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, Comment>> GetCommentsByIdsAsync(
        IReadOnlyList<Guid> commentIds, CancellationToken cancellationToken)
    {
        if (commentIds.Count == 0)
        {
            return new Dictionary<Guid, Comment>();
        }

        return await db.Comments
            .AsNoTracking()
            .Where(c => commentIds.Contains(c.Id) && c.DeletedAt == null)
            .ToDictionaryAsync(c => c.Id, cancellationToken);
    }

    public async Task AddCommentAsync(Comment comment, CancellationToken cancellationToken)
    {
        db.Comments.Add(comment);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveCommentAsync(Comment comment, CancellationToken cancellationToken)
    {
        if (db.Entry(comment).State == EntityState.Detached)
        {
            db.Comments.Update(comment);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ContentReportRecord>> GetOpenReportsAsync(CancellationToken cancellationToken) =>
        await db.ContentReports
            .AsNoTracking()
            .Where(r => r.Status == ReportStatus.Open)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<ContentReportRecord?> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken) =>
        db.ContentReports.SingleOrDefaultAsync(r => r.Id == reportId, cancellationToken);

    public async Task SaveReportAsync(ContentReportRecord report, CancellationToken cancellationToken)
    {
        if (db.Entry(report).State == EntityState.Detached)
        {
            db.ContentReports.Update(report);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var ownPostIds = await db.Posts.Where(p => p.UserId == userId).Select(p => p.Id).ToListAsync(cancellationToken);

        // Every comment on this user's own posts, regardless of who wrote them — the post is
        // about to be gone, so a comment on it makes no sense left behind.
        await db.Comments.Where(c => ownPostIds.Contains(c.PostId)).ExecuteDeleteAsync(cancellationToken);

        // This user's own comments on posts they don't own (already covered above if they did).
        await db.Comments.Where(c => c.UserId == userId).ExecuteDeleteAsync(cancellationToken);

        await db.Posts.Where(p => p.UserId == userId).ExecuteDeleteAsync(cancellationToken);
        await db.ContentReports.Where(r => r.ReporterUserId == userId).ExecuteDeleteAsync(cancellationToken);
    }
}
