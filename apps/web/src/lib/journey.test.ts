import { describe, it, expect } from "vitest";
import { computeJourneyNodes, buildDreamJourneyNodes } from "./journey";

describe("computeJourneyNodes", () => {
  it("marks a node with data as completed, not active, by default", () => {
    const [node] = computeJourneyNodes([
      { key: "a", label: "A", summary: "done", placeholder: "-", href: null, hasData: true },
    ]);
    expect(node.state).toBe("completed");
  });

  it("marks a node with data as active when isLive is set", () => {
    const [node] = computeJourneyNodes([
      { key: "a", label: "A", summary: "live thing", placeholder: "-", href: null, hasData: true, isLive: true },
    ]);
    expect(node.state).toBe("active");
  });

  it("marks the first node without data as upcoming, not empty", () => {
    const [first] = computeJourneyNodes([
      { key: "a", label: "A", summary: null, placeholder: "-", href: null, hasData: false },
      { key: "b", label: "B", summary: null, placeholder: "-", href: null, hasData: false },
    ]);
    expect(first.state).toBe("upcoming");
  });

  it("marks every node after the first gap as empty", () => {
    const [, second, third] = computeJourneyNodes([
      { key: "a", label: "A", summary: "done", placeholder: "-", href: null, hasData: true },
      { key: "b", label: "B", summary: null, placeholder: "-", href: null, hasData: false },
      { key: "c", label: "C", summary: null, placeholder: "-", href: null, hasData: false },
    ]);
    expect(second.state).toBe("upcoming");
    expect(third.state).toBe("empty");
  });

  it("never demotes a node that has real data, even after an earlier gap", () => {
    // Defensive case only - this app's data model can't produce it today, but the algorithm
    // should still do the honest thing if it ever did.
    const nodes = computeJourneyNodes([
      { key: "a", label: "A", summary: null, placeholder: "-", href: null, hasData: false },
      { key: "b", label: "B", summary: "has data anyway", placeholder: "-", href: null, hasData: true },
    ]);
    expect(nodes[0].state).toBe("upcoming");
    expect(nodes[1].state).toBe("completed");
  });

  it("always attaches a plain-language state label, never relying on color alone", () => {
    const nodes = computeJourneyNodes([
      { key: "a", label: "A", summary: "x", placeholder: "-", href: null, hasData: true },
      { key: "b", label: "B", summary: null, placeholder: "-", href: null, hasData: false },
      { key: "c", label: "C", summary: null, placeholder: "-", href: null, hasData: false },
    ]);
    expect(nodes.map((n) => n.stateLabel)).toEqual(["Completed", "Up next", "Not started yet"]);
  });
});

describe("buildDreamJourneyNodes", () => {
  it("builds all 8 stages in the required order", () => {
    const nodes = buildDreamJourneyNodes({
      dreamTitle: "A studio for small shop owners",
      fiveYearVision: null,
      threeYearDirection: null,
      oneYearGoal: null,
      ninetyDayMission: null,
      nextMoveTitle: null,
      activeExperimentSummary: null,
      learningsCount: 0,
    });

    expect(nodes.map((n) => n.key)).toEqual([
      "dream",
      "vision",
      "direction",
      "goal",
      "mission",
      "nextMove",
      "experiment",
      "learning",
    ]);
  });

  it("a brand new dream with no plan yet: dream active, vision is the only upcoming node, everything else empty", () => {
    const nodes = buildDreamJourneyNodes({
      dreamTitle: "A studio for small shop owners",
      fiveYearVision: null,
      threeYearDirection: null,
      oneYearGoal: null,
      ninetyDayMission: null,
      nextMoveTitle: null,
      activeExperimentSummary: null,
      learningsCount: 0,
    });

    const stateByKey = Object.fromEntries(nodes.map((n) => [n.key, n.state]));
    expect(stateByKey).toEqual({
      dream: "active",
      vision: "upcoming",
      direction: "empty",
      goal: "empty",
      mission: "empty",
      nextMove: "empty",
      experiment: "empty",
      learning: "empty",
    });
  });

  it("a drafted plan with no actions yet: all four plan stages completed, next move is upcoming", () => {
    const nodes = buildDreamJourneyNodes({
      dreamTitle: "A studio for small shop owners",
      fiveYearVision: "Real, thriving small shops everywhere.",
      threeYearDirection: "A trusted tool small shop owners recommend to each other.",
      oneYearGoal: "50 paying shops.",
      ninetyDayMission: "Validate the core idea with real conversations.",
      nextMoveTitle: null,
      activeExperimentSummary: null,
      learningsCount: 0,
    });

    const stateByKey = Object.fromEntries(nodes.map((n) => [n.key, n.state]));
    expect(stateByKey.vision).toBe("completed");
    expect(stateByKey.direction).toBe("completed");
    expect(stateByKey.goal).toBe("completed");
    expect(stateByKey.mission).toBe("completed");
    expect(stateByKey.nextMove).toBe("upcoming");
    expect(stateByKey.experiment).toBe("empty");
  });

  it("a fully active dream: next move and experiment are active, learning is completed, and real data is carried through untouched", () => {
    const nodes = buildDreamJourneyNodes({
      dreamTitle: "A studio for small shop owners",
      fiveYearVision: "Real, thriving small shops everywhere.",
      threeYearDirection: "A trusted tool small shop owners recommend to each other.",
      oneYearGoal: "50 paying shops.",
      ninetyDayMission: "Validate the core idea with real conversations.",
      nextMoveTitle: "Talk to five shop owners about invoicing pain",
      activeExperimentSummary: "Test a one-page invoicing mockup with 3 shop owners.",
      learningsCount: 2,
    });

    const byKey = Object.fromEntries(nodes.map((n) => [n.key, n]));
    expect(byKey.nextMove.state).toBe("active");
    expect(byKey.nextMove.summary).toBe("Talk to five shop owners about invoicing pain");
    expect(byKey.experiment.state).toBe("active");
    expect(byKey.learning.state).toBe("completed");
    expect(byKey.learning.summary).toBe("2 learnings recorded");
  });

  it("singularizes the learning count correctly", () => {
    const nodes = buildDreamJourneyNodes({
      dreamTitle: "A studio for small shop owners",
      fiveYearVision: null,
      threeYearDirection: null,
      oneYearGoal: null,
      ninetyDayMission: null,
      nextMoveTitle: null,
      activeExperimentSummary: null,
      learningsCount: 1,
    });

    const learning = nodes.find((n) => n.key === "learning");
    expect(learning?.summary).toBe("1 learning recorded");
  });

  it("gives every non-dream node a working href to its existing detail section, and none to the dream node itself", () => {
    const nodes = buildDreamJourneyNodes({
      dreamTitle: "A studio for small shop owners",
      fiveYearVision: null,
      threeYearDirection: null,
      oneYearGoal: null,
      ninetyDayMission: null,
      nextMoveTitle: null,
      activeExperimentSummary: null,
      learningsCount: 0,
    });

    const byKey = Object.fromEntries(nodes.map((n) => [n.key, n]));
    expect(byKey.dream.href).toBeNull();
    expect(byKey.vision.href).toBe("/app/plan");
    expect(byKey.nextMove.href).toBe("#next-move");
    expect(byKey.experiment.href).toBe("#active-experiment");
    expect(byKey.learning.href).toBe("#learnings");
  });
});
