using Waypoint.BusinessIdeas.Domain;

namespace Waypoint.BusinessIdeas.Application;

public interface IBusinessIdeasRepository
{
    Task<BusinessIdea?> GetForDreamAsync(Guid dreamId, CancellationToken cancellationToken);
    Task AddAsync(BusinessIdea idea, CancellationToken cancellationToken);
    Task SaveAsync(BusinessIdea idea, CancellationToken cancellationToken);

    Task<IReadOnlyList<BusinessValidation>> GetValidationsForIdeaAsync(Guid businessIdeaId, CancellationToken cancellationToken);
    Task AddValidationAsync(BusinessValidation validation, CancellationToken cancellationToken);
}
