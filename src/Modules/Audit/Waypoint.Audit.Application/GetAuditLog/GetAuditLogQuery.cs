using MediatR;

namespace Waypoint.Audit.Application.GetAuditLog;

public sealed record GetAuditLogQuery(int Take = 200) : IRequest<IReadOnlyList<AuditLogEntryDto>>;

public sealed class GetAuditLogQueryHandler(IAuditLogRepository repository)
    : IRequestHandler<GetAuditLogQuery, IReadOnlyList<AuditLogEntryDto>>
{
    public async Task<IReadOnlyList<AuditLogEntryDto>> Handle(GetAuditLogQuery request, CancellationToken cancellationToken)
    {
        var take = Math.Clamp(request.Take, 1, 500);
        var entries = await repository.GetRecentAsync(take, cancellationToken);
        return entries.Select(AuditLogEntryDto.From).ToList();
    }
}
