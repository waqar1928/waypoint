export type JourneyNodeState = "completed" | "active" | "upcoming" | "empty";

export interface JourneyNodeInput {
  key: string;
  label: string;
  /** Real, existing-domain text to show for this stage, or null if nothing exists yet. */
  summary: string | null;
  /** Shown instead of summary when there's nothing yet - never phrased as an error. */
  placeholder: string;
  /** Where clicking/focusing this node should take the user, or null if it isn't a link
   * (the Dream node itself - you're already on its page). */
  href: string | null;
}

export interface JourneyNode extends JourneyNodeInput {
  state: JourneyNodeState;
  /** Plain-language state label always rendered as visible text alongside the icon, so state is
   * never communicated by color/icon alone. */
  stateLabel: string;
}

const stateLabels: Record<JourneyNodeState, string> = {
  completed: "Completed",
  active: "In progress",
  upcoming: "Up next",
  empty: "Not started yet",
};

/**
 * Computes each Dream Journey Rail node's display state from data the app already has - no new
 * queries, no invented business rules, no scoring/recommendation logic (that stays exclusively in
 * NextBestActionSelector on the backend). `hasData` says whether real content exists for that
 * stage; `isLive` marks the kinds of node that are inherently an ongoing/current thing rather than
 * a checkbox to tick (the Dream itself, and whichever Action/Experiment is presently the live
 * one) - those get "active" instead of "completed" when they have data.
 *
 * Walks the fixed stage sequence once: the first stage with no data yet is "upcoming" (a single,
 * honest "what's next" pointer, never more than one at a time); anything further down the
 * sequence that also has no data is "empty" - not broken, just not reached yet and not the
 * immediate next step either, so it must never be styled like an error state. A later stage that
 * DOES have data is never downgraded just because an earlier one is missing (defensive only - this
 * app's current data model can't actually produce that shape, since the 5-Year Vision / 3-Year
 * Direction / 1-Year Goal / 90-Day Mission are always drafted and saved together as one Plan).
 */
export function computeJourneyNodes(
  inputs: (JourneyNodeInput & { hasData: boolean; isLive?: boolean })[],
): JourneyNode[] {
  let seenGap = false;
  return inputs.map(({ hasData, isLive, ...node }) => {
    let state: JourneyNodeState;
    if (hasData) {
      state = isLive ? "active" : "completed";
    } else if (!seenGap) {
      state = "upcoming";
      seenGap = true;
    } else {
      state = "empty";
    }
    return { ...node, state, stateLabel: stateLabels[state] };
  });
}

export interface DreamJourneyInput {
  dreamTitle: string;
  fiveYearVision: string | null;
  threeYearDirection: string | null;
  oneYearGoal: string | null;
  ninetyDayMission: string | null;
  nextMoveTitle: string | null;
  activeExperimentSummary: string | null;
  learningsCount: number;
}

/** Builds the 8 Dream Journey Rail nodes (Dream -> Vision -> Direction -> Goal -> Mission ->
 * Next Move -> Experiment -> Learning) from data the Dream Overview page already fetches.
 * Vision/Direction/Goal/Mission link to the Plan page, where they're actually edited; Next
 * Move/Experiment/Learning link to their existing detail cards further down this same page -
 * the rail is a navigation/summary layer over that content, not a replacement for it. */
export function buildDreamJourneyNodes(input: DreamJourneyInput): JourneyNode[] {
  return computeJourneyNodes([
    {
      key: "dream",
      label: "Dream",
      summary: input.dreamTitle,
      placeholder: input.dreamTitle,
      href: null,
      hasData: true,
      isLive: true,
    },
    {
      key: "vision",
      label: "5-Year Vision",
      summary: input.fiveYearVision,
      placeholder: "Not drafted yet.",
      href: "/app/plan",
      hasData: input.fiveYearVision !== null,
    },
    {
      key: "direction",
      label: "3-Year Direction",
      summary: input.threeYearDirection,
      placeholder: "Not drafted yet.",
      href: "/app/plan",
      hasData: input.threeYearDirection !== null,
    },
    {
      key: "goal",
      label: "1-Year Goal",
      summary: input.oneYearGoal,
      placeholder: "Not drafted yet.",
      href: "/app/plan",
      hasData: input.oneYearGoal !== null,
    },
    {
      key: "mission",
      label: "90-Day Mission",
      summary: input.ninetyDayMission,
      placeholder: "Not drafted yet.",
      href: "/app/plan",
      hasData: input.ninetyDayMission !== null,
    },
    {
      key: "nextMove",
      label: "Next Move",
      summary: input.nextMoveTitle,
      placeholder: "No action recommended yet.",
      href: "#next-move",
      hasData: input.nextMoveTitle !== null,
      isLive: true,
    },
    {
      key: "experiment",
      label: "Experiment",
      summary: input.activeExperimentSummary,
      placeholder: "No experiment running.",
      href: "#active-experiment",
      hasData: input.activeExperimentSummary !== null,
      isLive: true,
    },
    {
      key: "learning",
      label: "Learning",
      summary:
        input.learningsCount > 0
          ? `${input.learningsCount} ${input.learningsCount === 1 ? "learning" : "learnings"} recorded`
          : null,
      placeholder: "None recorded yet.",
      href: "#learnings",
      hasData: input.learningsCount > 0,
    },
  ]);
}
