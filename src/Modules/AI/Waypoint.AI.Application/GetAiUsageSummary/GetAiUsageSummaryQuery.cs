using MediatR;

namespace Waypoint.AI.Application.GetAiUsageSummary;

public sealed record GetAiUsageSummaryQuery : IRequest<AiUsageSummaryDto>;

public sealed class GetAiUsageSummaryQueryHandler(IAiRepository repository)
    : IRequestHandler<GetAiUsageSummaryQuery, AiUsageSummaryDto>
{
    public Task<AiUsageSummaryDto> Handle(GetAiUsageSummaryQuery request, CancellationToken cancellationToken) =>
        repository.GetUsageSummaryAsync(cancellationToken);
}
