// Deliberately its own module, not folded into lib/actions.ts: actions.ts imports
// `next/headers` (server-only) for its fetch functions, and that import poisons the whole
// module for any Client Component that imports anything from it at all - even a pure function
// with no side effects. dream-overview.tsx is a Client Component and needs this function, so it
// lives here instead, with only a type-only import of WaypointAction (erased at compile time,
// so it carries none of actions.ts's runtime baggage into the client bundle).
import type { WaypointAction } from "@/lib/actions";

/**
 * The single place that decides what secondary text to show under a recommended action's
 * title. The Dashboard's "Next best action" card and the Dream Overview's "Next move" card
 * both call this instead of each independently picking a field off the action object - that's
 * exactly how they drifted before this existed: Dashboard showed `description` (the action's
 * own free-text field, usually empty), Dream Overview showed `rationale` (the computed "why
 * this one" reasoning from NextBestActionSelector on the backend). Same underlying
 * recommendation, two different answers to "what do we show," because the decision lived in
 * two separate JSX branches instead of one function. Pulling it out here means there's now
 * exactly one place that can be wrong, and it's covered by a unit test.
 */
export function nextMoveRationaleText(action: WaypointAction | null): string | null {
  return action?.rationale ?? null;
}
