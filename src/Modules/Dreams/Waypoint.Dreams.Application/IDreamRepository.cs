using Waypoint.Dreams.Domain;

namespace Waypoint.Dreams.Application;

public interface IDreamRepository
{
    Task<Dream?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> ExistsForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task SaveAsync(Dream dream, CancellationToken cancellationToken);
}
