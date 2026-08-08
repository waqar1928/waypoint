using Waypoint.Common;

namespace Waypoint.Goals.Application;

/// <summary>
/// Turns a Dream Statement into a draft 5yr/3yr/1yr/90-day cascade. Phase 3
/// ships a deterministic heuristic; Phase 6 can swap in an AI-backed
/// implementation behind this same interface — same pattern as
/// IDreamDirectionGenerator in the Dreams module.
/// </summary>
public interface IPlanDraftGenerator
{
    PlanDraftDto Generate(DreamSummary dream);
}
