using Microsoft.EntityFrameworkCore;
using Waypoint.Mentorship.Application;
using Waypoint.Mentorship.Domain;

namespace Waypoint.Mentorship.Infrastructure;

public sealed class MentorshipRepository(MentorshipDbContext db) : IMentorshipRepository
{
    public Task<MentorProfile?> GetMentorProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        db.MentorProfiles.SingleOrDefaultAsync(m => m.UserId == userId, cancellationToken);

    public Task<MentorProfile?> GetMentorProfileByIdAsync(Guid mentorProfileId, CancellationToken cancellationToken) =>
        db.MentorProfiles.SingleOrDefaultAsync(m => m.Id == mentorProfileId, cancellationToken);

    public async Task<IReadOnlyList<MentorProfile>> GetMentorDirectoryAsync(
        string? expertiseFilter, CancellationToken cancellationToken)
    {
        // Expertise is a jsonb-mapped List<string> — filtering "does this list contain X" isn't
        // translatable to SQL through the value converter, so fetch and filter in memory. Fine at
        // this app's scale; revisit with a real search index if the mentor pool grows large.
        var profiles = await db.MentorProfiles.ToListAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(expertiseFilter))
        {
            return profiles;
        }

        return profiles
            .Where(p => p.Expertise.Any(e => e.Contains(expertiseFilter, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task AddMentorProfileAsync(MentorProfile profile, CancellationToken cancellationToken)
    {
        db.MentorProfiles.Add(profile);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveMentorProfileAsync(MentorProfile profile, CancellationToken cancellationToken)
    {
        if (db.Entry(profile).State == EntityState.Detached)
        {
            db.MentorProfiles.Update(profile);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<HelpRequest?> GetHelpRequestByIdAsync(Guid helpRequestId, CancellationToken cancellationToken) =>
        db.HelpRequests.SingleOrDefaultAsync(h => h.Id == helpRequestId, cancellationToken);

    public async Task<IReadOnlyList<HelpRequest>> GetHelpRequestsAsync(
        HelpRequestCategory? categoryFilter, HelpRequestStatus? statusFilter, CancellationToken cancellationToken)
    {
        var query = db.HelpRequests.AsQueryable();
        if (categoryFilter is not null)
        {
            query = query.Where(h => h.Category == categoryFilter);
        }
        if (statusFilter is not null)
        {
            query = query.Where(h => h.Status == statusFilter);
        }
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HelpRequest>> GetHelpRequestsForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.HelpRequests.Where(h => h.UserId == userId).ToListAsync(cancellationToken);

    public async Task AddHelpRequestAsync(HelpRequest request, CancellationToken cancellationToken)
    {
        db.HelpRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveHelpRequestAsync(HelpRequest request, CancellationToken cancellationToken)
    {
        if (db.Entry(request).State == EntityState.Detached)
        {
            db.HelpRequests.Update(request);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetResponseCountsAsync(
        IReadOnlyList<Guid> helpRequestIds, CancellationToken cancellationToken)
    {
        if (helpRequestIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        return await db.HelpRequestResponses
            .Where(r => helpRequestIds.Contains(r.HelpRequestId))
            .GroupBy(r => r.HelpRequestId)
            .Select(g => new { HelpRequestId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.HelpRequestId, x => x.Count, cancellationToken);
    }

    public async Task<IReadOnlyList<HelpRequestResponse>> GetResponsesForHelpRequestAsync(
        Guid helpRequestId, CancellationToken cancellationToken) =>
        await db.HelpRequestResponses
            .Where(r => r.HelpRequestId == helpRequestId)
            .ToListAsync(cancellationToken);

    public async Task AddResponseAsync(HelpRequestResponse response, CancellationToken cancellationToken)
    {
        db.HelpRequestResponses.Add(response);
        await db.SaveChangesAsync(cancellationToken);
    }
}
