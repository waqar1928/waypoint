"use client";

import { useState } from "react";
import Link from "next/link";
import { ArrowRight } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Button, buttonClasses } from "@/components/ui/button";
import { Input } from "@/components/ui/field";
import { ActionReflectionPrompt } from "@/components/actions/action-reflection-prompt";
import { apiMutate } from "@/lib/api-client";
import { nextMoveRationaleText } from "@/lib/next-move";
import type { WaypointAction } from "@/lib/actions";

/**
 * The interactive "Next move" card on Dream Overview: What (title), Why (the computed rationale -
 * same single source of truth Dashboard reads, see lib/next-move.ts), and Time estimate, plus
 * Start / Complete / Reschedule using the existing Actions status and update endpoints -
 * UpdateActionStatusCommand and UpdateActionCommand on the backend, nothing new. Deliberately no
 * Skip button: that would need a new status or domain concept that doesn't exist today, which is
 * explicitly out of scope for this round of work.
 */
export function NextMoveCard({ initialAction }: { initialAction: WaypointAction | null }) {
  const [action, setAction] = useState(initialAction);
  const [isRescheduling, setIsRescheduling] = useState(false);
  const [rescheduleDate, setRescheduleDate] = useState(initialAction?.dueDate ?? "");
  // Independent of `action` on purpose: completing an action immediately looks up whatever is
  // next best now (which may be a different action, or none), but the reflection prompt still
  // needs to know which action it's actually being saved against.
  const [reflectingActionId, setReflectingActionId] = useState<string | null>(null);
  const [justCompletedTitle, setJustCompletedTitle] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const refreshNextBest = async () => {
    try {
      const response = await fetch("/api/actions/next-best");
      const nextBest = response.ok ? ((await response.json()) as WaypointAction) : null;
      setAction(nextBest);
      setRescheduleDate(nextBest?.dueDate ?? "");
    } catch {
      // Non-critical: the card just keeps showing its last known state until the next
      // mutation or page load succeeds in refreshing it.
    }
  };

  // UpdateActionStatusCommand and UpdateActionCommand both return a plain ActionDto, which only
  // ever carries a rationale when it comes from GetNextBestActionQuery (see the doc comment on
  // WaypointAction.rationale in lib/actions.ts) - so a Start or Reschedule response always comes
  // back with rationale: null, even though the action is still the same recommendation for the
  // same reason. Carry the rationale we already know forward rather than dropping it, since
  // neither of those two actions is what changed it.
  const withPreservedRationale = (updated: WaypointAction): WaypointAction => ({
    ...updated,
    rationale: action?.id === updated.id ? action.rationale : updated.rationale,
  });

  const handleStart = async () => {
    if (!action) return;
    setError(null);
    const response = await apiMutate(`/api/actions/${action.id}/status`, {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ status: "inProgress" }),
    });
    if (!response.ok) {
      setError("We couldn't start that. Please try again.");
      return;
    }
    setAction(withPreservedRationale((await response.json()) as WaypointAction));
  };

  const handleComplete = async () => {
    if (!action) return;
    setError(null);
    const response = await apiMutate(`/api/actions/${action.id}/status`, {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ status: "completed" }),
    });
    if (!response.ok) {
      setError("We couldn't complete that. Please try again.");
      return;
    }
    setReflectingActionId(action.id);
    setJustCompletedTitle(action.title);
    void refreshNextBest();
  };

  const handleAddReflection = async (whatHappened: string, learning: string) => {
    if (!reflectingActionId) return;
    const response = await apiMutate(`/api/actions/${reflectingActionId}/reflection`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ whatHappened: whatHappened || null, learning: learning || null }),
    });
    if (!response.ok) {
      setError("We couldn't save that. Please try again.");
      return;
    }
    setReflectingActionId(null);
    setJustCompletedTitle(null);
  };

  const handleReschedule = async () => {
    if (!action) return;
    setError(null);
    // UpdateActionCommand replaces every editable field, not just DueDate - resend the action's
    // current values for everything else unchanged.
    const response = await apiMutate(`/api/actions/${action.id}`, {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({
        title: action.title,
        description: action.description,
        priority: action.priority,
        difficulty: action.difficulty,
        estimatedMinutes: action.estimatedMinutes,
        expectedImpact: action.expectedImpact,
        dueDate: rescheduleDate || null,
      }),
    });
    if (!response.ok) {
      setError("We couldn't reschedule that. Please try again.");
      return;
    }
    setAction(withPreservedRationale((await response.json()) as WaypointAction));
    setIsRescheduling(false);
  };

  const rationale = nextMoveRationaleText(action);
  const canStart = action?.status === "notStarted";
  const canComplete = action?.status === "notStarted" || action?.status === "inProgress";
  const canReschedule = canComplete;

  return (
    <Card id="next-move" className="border-beacon-500/40 bg-beacon-100/40">
      <p className="text-xs font-semibold uppercase tracking-wide text-beacon-600">Next move</p>

      {action ? (
        <>
          <h2 className="mt-2 font-display text-lg font-semibold text-ink-900">{action.title}</h2>
          {rationale ? <p className="mt-1 text-sm text-beacon-700">{rationale}</p> : null}
          {action.estimatedMinutes ? (
            <p className="mt-2 text-xs text-ink-500">About {action.estimatedMinutes} min</p>
          ) : null}
          {action.dueDate ? (
            <p className="mt-1 text-xs text-ink-500">
              Due {new Date(action.dueDate).toLocaleDateString("en-US")}
            </p>
          ) : null}

          {error ? (
            <p role="alert" className="mt-2 text-sm text-merlot-600">
              {error}
            </p>
          ) : null}

          <div className="mt-4 flex flex-wrap items-center gap-3">
            {canStart ? (
              <Button type="button" variant="secondary" onClick={handleStart}>
                Start
              </Button>
            ) : null}
            {canComplete ? (
              <Button type="button" onClick={handleComplete}>
                Complete
              </Button>
            ) : null}
            {canReschedule && !isRescheduling ? (
              <button
                type="button"
                onClick={() => setIsRescheduling(true)}
                className="text-sm font-medium text-beacon-600 hover:underline"
              >
                Reschedule
              </button>
            ) : null}
            <Link href="/app/actions" className={buttonClasses("ghost", "gap-2")}>
              Go to Actions
              <ArrowRight className="h-4 w-4" aria-hidden="true" />
            </Link>
          </div>

          {isRescheduling ? (
            <div className="mt-3 flex flex-wrap items-center gap-2">
              <Input
                type="date"
                value={rescheduleDate ?? ""}
                onChange={(e) => setRescheduleDate(e.target.value)}
                aria-label="New due date"
                className="w-auto"
              />
              <Button type="button" className="px-3 py-1.5 text-xs" onClick={handleReschedule}>
                Save date
              </Button>
              <button
                type="button"
                onClick={() => setIsRescheduling(false)}
                className="text-xs font-medium text-ink-500 hover:text-ink-900"
              >
                Cancel
              </button>
            </div>
          ) : null}
        </>
      ) : (
        <>
          <h2 className="mt-2 font-display text-lg font-semibold text-ink-900">Add your first action</h2>
          <p className="mt-1 text-sm text-ink-700">A small, concrete next step you can start on.</p>
          <Link href="/app/actions" className={buttonClasses("primary", "mt-4 gap-2")}>
            Go to Actions
            <ArrowRight className="h-4 w-4" aria-hidden="true" />
          </Link>
        </>
      )}

      {/* Deliberately outside the action ? ... : ... branch above: completing the last open
          action makes `action` refresh to null (no more recommendation), which would otherwise
          make this whole prompt vanish along with the "next move" content it has nothing to do
          with. reflectingActionId/justCompletedTitle are already tracked independently of
          `action` for exactly this reason (see their declaration above) - this only fixes where
          they're rendered to actually match that intent. */}
      {reflectingActionId ? (
        <>
          {justCompletedTitle ? (
            <p className="mt-4 border-t border-ink-300 pt-4 text-sm text-ink-700">
              Nice work finishing &ldquo;{justCompletedTitle}&rdquo;.
            </p>
          ) : null}
          <ActionReflectionPrompt
            actionId={reflectingActionId}
            onSave={handleAddReflection}
            onSkip={() => {
              setReflectingActionId(null);
              setJustCompletedTitle(null);
            }}
          />
        </>
      ) : null}
    </Card>
  );
}
