"use client";

import { clsx } from "clsx";
import { Star, CheckCircle2 } from "lucide-react";
import { Card } from "@/components/ui/card";
import { ActionReflectionPrompt } from "@/components/actions/action-reflection-prompt";
import type { ActionStatus, WaypointAction } from "@/lib/actions";

const statusLabels: Record<ActionStatus, string> = {
  notStarted: "Not started",
  inProgress: "In progress",
  completed: "Completed",
  blocked: "Blocked",
  cancelled: "Cancelled",
};

const impactStyles: Record<string, string> = {
  low: "bg-ink-100 text-ink-700",
  medium: "bg-amber-100 text-amber-600",
  high: "bg-sage-100 text-sage-600",
};

export function ActionRow({
  action,
  isNextBest,
  nextBestRationale,
  onStatusChange,
  onSetNextBest,
  isReflecting,
  onAddReflection,
  onDismissReflection,
}: {
  action: WaypointAction;
  /** Whether this row is THE computed next best action right now - a manual pin if one exists,
   * otherwise NextBestActionSelector's pick (the same single source of truth Dashboard and Dream
   * Overview read). Deliberately not action.isNextBestAction: that field only reflects a manual
   * pin, so it used to miss the computed-fallback case entirely. */
  isNextBest: boolean;
  /** The computed rationale for this row, already resolved by the caller via
   * nextMoveRationaleText() - null unless isNextBest is true. */
  nextBestRationale: string | null;
  onStatusChange: (status: ActionStatus) => void;
  onSetNextBest: () => void;
  isReflecting: boolean;
  onAddReflection: (whatHappened: string, learning: string) => void;
  onDismissReflection: () => void;
}) {
  const isDone = action.status === "completed" || action.status === "cancelled";

  return (
    <Card className={clsx("transition-colors duration-300 ease-out", isNextBest && "border-beacon-500 bg-beacon-100/30")}>
      <div className="flex items-start justify-between gap-4">
        <div className="min-w-0 flex-1">
          <div className="flex items-center gap-2">
            {isNextBest ? (
              <span className="flex items-center gap-1 rounded-full bg-beacon-500 px-2.5 py-0.5 text-xs font-semibold text-white">
                <Star className="h-3 w-3" aria-hidden="true" />
                Next best
              </span>
            ) : null}
            {action.status === "completed" ? (
              // A brief, quiet acknowledgement that this just got done - not a celebration, just
              // a settle-in. See the drevia-settle keyframe in globals.css.
              <CheckCircle2
                className="h-4 w-4 shrink-0 animate-[drevia-settle_260ms_ease-out] text-sage-600"
                aria-hidden="true"
              />
            ) : null}
            <h3
              className={clsx(
                "font-display text-base font-semibold text-ink-900 transition-colors duration-300 ease-out",
                isDone && "line-through text-ink-500",
              )}
            >
              {action.title}
            </h3>
          </div>
          {action.description ? <p className="mt-1 text-sm text-ink-700">{action.description}</p> : null}
          {isNextBest && nextBestRationale ? (
            <p className="mt-1 text-xs text-beacon-600">{nextBestRationale}</p>
          ) : null}

          <div className="mt-3 flex flex-wrap items-center gap-2 text-xs">
            <span className={clsx("rounded-full px-2 py-0.5 font-medium", impactStyles[action.expectedImpact])}>
              {action.expectedImpact} impact
            </span>
            <span className="rounded-full bg-ink-100 px-2 py-0.5 text-ink-700">{action.priority} priority</span>
            <span className="rounded-full bg-ink-100 px-2 py-0.5 text-ink-700">{action.difficulty}</span>
            {action.estimatedMinutes ? (
              <span className="rounded-full bg-ink-100 px-2 py-0.5 text-ink-700">~{action.estimatedMinutes} min</span>
            ) : null}
            {action.dueDate ? (
              <span className="rounded-full bg-ink-100 px-2 py-0.5 text-ink-700">
                Due {new Date(action.dueDate).toLocaleDateString("en-US")}
              </span>
            ) : null}
          </div>
        </div>

        <div className="flex shrink-0 flex-col items-end gap-2">
          <select
            aria-label={`Status for ${action.title}`}
            value={action.status}
            onChange={(e) => onStatusChange(e.target.value as ActionStatus)}
            className="min-h-9 rounded-[10px] border border-ink-300 bg-paper-raised px-2 text-xs text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
          >
            {Object.entries(statusLabels).map(([value, label]) => (
              <option key={value} value={value}>
                {label}
              </option>
            ))}
          </select>
          {!isNextBest && !isDone ? (
            <button
              type="button"
              onClick={onSetNextBest}
              className="text-xs font-medium text-beacon-600 hover:underline"
            >
              Make this next
            </button>
          ) : null}
        </div>
      </div>

      {isReflecting ? (
        <ActionReflectionPrompt actionId={action.id} onSave={onAddReflection} onSkip={onDismissReflection} />
      ) : null}
    </Card>
  );
}
