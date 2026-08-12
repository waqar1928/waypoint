using Microsoft.EntityFrameworkCore;
using Waypoint.BusinessIdeas.Application;
using Waypoint.BusinessIdeas.Domain;

namespace Waypoint.BusinessIdeas.Infrastructure;

public sealed class BusinessIdeasRepository(BusinessIdeasDbContext db) : IBusinessIdeasRepository
{
    public Task<BusinessIdea?> GetForDreamAsync(Guid dreamId, CancellationToken cancellationToken) =>
        db.BusinessIdeas.SingleOrDefaultAsync(i => i.DreamId == dreamId, cancellationToken);

    public async Task AddAsync(BusinessIdea idea, CancellationToken cancellationToken)
    {
        db.BusinessIdeas.Add(idea);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(BusinessIdea idea, CancellationToken cancellationToken)
    {
        if (db.Entry(idea).State == EntityState.Detached)
        {
            db.BusinessIdeas.Update(idea);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BusinessValidation>> GetValidationsForIdeaAsync(
        Guid businessIdeaId, CancellationToken cancellationToken) =>
        await db.BusinessValidations
            .AsNoTracking()
            .Where(v => v.BusinessIdeaId == businessIdeaId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddValidationAsync(BusinessValidation validation, CancellationToken cancellationToken)
    {
        db.BusinessValidations.Add(validation);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteForDreamAsync(Guid dreamId, CancellationToken cancellationToken)
    {
        var ideaIds = await db.BusinessIdeas.Where(i => i.DreamId == dreamId).Select(i => i.Id).ToListAsync(cancellationToken);
        await db.BusinessValidations.Where(v => ideaIds.Contains(v.BusinessIdeaId)).ExecuteDeleteAsync(cancellationToken);
        await db.BusinessIdeas.Where(i => i.DreamId == dreamId).ExecuteDeleteAsync(cancellationToken);
    }
}
