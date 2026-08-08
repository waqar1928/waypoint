using MediatR;

namespace Waypoint.Dreams.Application.GenerateDreamDirections;

public sealed record GenerateDreamDirectionsQuery(DiscoveryAnswers Answers) : IRequest<IReadOnlyList<DreamDirectionDto>>;

public sealed class GenerateDreamDirectionsQueryHandler(IDreamDirectionGenerator generator)
    : IRequestHandler<GenerateDreamDirectionsQuery, IReadOnlyList<DreamDirectionDto>>
{
    public Task<IReadOnlyList<DreamDirectionDto>> Handle(
        GenerateDreamDirectionsQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(generator.Generate(request.Answers));
}
