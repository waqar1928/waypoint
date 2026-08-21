import { describe, it, expect } from "vitest";
import { nextMoveRationaleText } from "./next-move";
import type { WaypointAction } from "./actions";

function makeAction(overrides: Partial<WaypointAction> = {}): WaypointAction {
  return {
    id: "1",
    title: "Talk to five shop owners about invoicing pain",
    description: "Some unrelated free-text note the user wrote when creating the action.",
    priority: "high",
    difficulty: "medium",
    estimatedMinutes: null,
    expectedImpact: "high",
    dueDate: null,
    status: "notStarted",
    isNextBestAction: false,
    goalId: null,
    missionId: null,
    rationale: null,
    ...overrides,
  };
}

describe("nextMoveRationaleText", () => {
  it("returns the computed rationale, not the action's own description", () => {
    const action = makeAction({
      description: "Some unrelated free-text note the user wrote when creating the action.",
      rationale: "This is next because it's high priority and it's likely to move things forward the most.",
    });

    expect(nextMoveRationaleText(action)).toBe(
      "This is next because it's high priority and it's likely to move things forward the most.",
    );
  });

  it("returns null when there's no rationale, even if a description exists", () => {
    // Regression guard for the exact bug this function fixes: a non-null `description` must
    // never leak through as a substitute for a missing `rationale`.
    const action = makeAction({ description: "A note", rationale: null });

    expect(nextMoveRationaleText(action)).toBeNull();
  });

  it("returns null when there is no action at all", () => {
    expect(nextMoveRationaleText(null)).toBeNull();
  });

  it("is the same answer regardless of which screen asks - this is what makes it a single source of truth", () => {
    const action = makeAction({ rationale: "You marked this as your next move." });

    // Dashboard and Dream Overview both call this same function with the same fetched action;
    // proving it's deterministic (same input, same output, called twice) is what "one source of
    // truth" actually means for a pure function - there's no hidden state to diverge on.
    const dashboardText = nextMoveRationaleText(action);
    const dreamOverviewText = nextMoveRationaleText(action);

    expect(dashboardText).toBe(dreamOverviewText);
    expect(dashboardText).toBe("You marked this as your next move.");
  });
});
