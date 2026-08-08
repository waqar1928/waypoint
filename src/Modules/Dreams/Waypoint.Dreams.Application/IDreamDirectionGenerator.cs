namespace Waypoint.Dreams.Application;

/// <summary>
/// Turns Discovery answers into candidate Dream Directions. Phase 2 ships
/// a deterministic heuristic (HeuristicDreamDirectionGenerator); Phase 6
/// swaps in an AI-backed implementation behind this same interface — no
/// change needed to the command/handler or the frontend contract.
/// </summary>
public interface IDreamDirectionGenerator
{
    IReadOnlyList<DreamDirectionDto> Generate(DiscoveryAnswers answers);
}
