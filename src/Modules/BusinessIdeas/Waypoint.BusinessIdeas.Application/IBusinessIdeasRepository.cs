using Waypoint.BusinessIdeas.Domain;

namespace Waypoint.BusinessIdeas.Application;

public interface IBusinessIdeasRepository
{
    Task<BusinessIdea?> GetForDreamAsync(Guid dreamId, CancellationToken cancellationToken);
    Task AddAsync(BusinessIdea idea, CancellationToken cancellationToken);
    Task SaveAsync(BusinessIdea idea, CancellationToken cancellationToken);

    Task<IReadOnlyList<BusinessValidation>> GetValidationsForIdeaAsync(Guid businessIdeaId, CancellationToken cancellationToken);
    Task AddValidationAsync(BusinessValidation validation, CancellationToken cancellationToken);

    /// <summary>Real hard delete of the BusinessIdea (at most one per dream) and its Validations
    /// for one dream — backs account deletion (see docs/PRODUCTION_READINESS_AUDIT.md's Data
    /// Protection section). BusinessValidation has no DB-level FK/cascade from BusinessIdea (only
    /// an index on BusinessIdeaId), so this deletes validations explicitly.</summary>
    Task DeleteForDreamAsync(Guid dreamId, CancellationToken cancellationToken);
}
