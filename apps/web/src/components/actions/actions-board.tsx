"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { ActionCreateForm } from "@/components/actions/action-create-form";
import { ActionRow } from "@/components/actions/action-row";
import { apiMutate } from "@/lib/api-client";
import { nextMoveRationaleText } from "@/lib/next-move";
import type { CreateActionInput } from "@/lib/validation";
import type { ActionStatus, WaypointAction } from "@/lib/actions";

export function ActionsBoard({
  initialActions,
  initialNextBestActionId,
  initialNextBestRationale,
}: {
  initialActions: WaypointAction[];
  initialNextBestActionId: string | null;
  initialNextBestRationale: string | null;
}) {
  const [actions, setActions] = useState(initialActions);
  const [isCreating, setIsCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  // Set only right after the user themselves marks an action completed (not on initial load, and
  // not for any other status change) - this is what triggers the optional "what happened / what
  // did you learn" prompt in ActionRow. Cleared by either submitting or skipping that prompt.
  const [reflectingActionId, setReflectingActionId] = useState<string | null>(null);
  // The COMPUTED next best action - the same thing GET /api/actions/next-best returns to
  // Dashboard and Dream Overview (a manual pin if one exists, otherwise NextBestActionSelector's
  // pick). This drives the "Next best" badge/rationale below, not each action's own
  // isNextBestAction field - that field only reflects a manual pin, so a computed-but-unpinned
  // recommendation used to show no indicator at all in this list before this existed.
  const [nextBestActionId, setNextBestActionId] = useState(initialNextBestActionId);
  const [nextBestRationale, setNextBestRationale] = useState(initialNextBestRationale);

  // Anything that could change which action is recommended (a new action added, any status
  // change, or a manual pin) re-asks the same single source of truth rather than trying to
  // predict the answer client-side.
  const refreshNextBest = async () => {
    try {
      const response = await fetch("/api/actions/next-best");
      const nextBest = response.ok ? ((await response.json()) as WaypointAction) : null;
      setNextBestActionId(nextBest?.id ?? null);
      setNextBestRationale(nextMoveRationaleText(nextBest));
    } catch {
      // Non-critical: the badge just keeps showing its last known state until the next
      // mutation or page load succeeds in refreshing it.
    }
  };

  const handleCreate = async (values: CreateActionInput) => {
    setError(null);
    const response = await apiMutate("/api/actions", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({
        ...values,
        dueDate: values.dueDate || null,
        estimatedMinutes: values.estimatedMinutes ? Number(values.estimatedMinutes) : null,
      }),
    });

    if (!response.ok) {
      setError("We couldn't add that action. Please try again.");
      return;
    }

    const created = (await response.json()) as WaypointAction;
    setActions((current) => [created, ...current]);
    setIsCreating(false);
    void refreshNextBest();
  };

  const handleStatusChange = async (actionId: string, status: ActionStatus) => {
    const response = await apiMutate(`/api/actions/${actionId}/status`, {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ status }),
    });
    if (!response.ok) {
      setError("We couldn't update that action's status. Please try again.");
      return;
    }
    const updated = (await response.json()) as WaypointAction;
    setActions((current) => current.map((a) => (a.id === actionId ? updated : a)));
    setReflectingActionId(status === "completed" ? actionId : null);
    void refreshNextBest();
  };

  const handleAddReflection = async (actionId: string, whatHappened: string, learning: string) => {
    const response = await apiMutate(`/api/actions/${actionId}/reflection`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ whatHappened: whatHappened || null, learning: learning || null }),
    });
    if (!response.ok) {
      setError("We couldn't save that. Please try again.");
      return;
    }
    setReflectingActionId(null);
  };

  const handleSetNextBest = async (actionId: string) => {
    const response = await apiMutate(`/api/actions/${actionId}/set-next-best`, { method: "POST" });
    if (!response.ok) {
      setError("We couldn't set that as your next best action. Please try again.");
      return;
    }
    const updated = (await response.json()) as WaypointAction;
    setActions((current) =>
      current.map((a) => (a.id === actionId ? updated : { ...a, isNextBestAction: false })),
    );
    // A manual pin always wins in the selector, so we can set this directly instead of
    // round-tripping through refreshNextBest() - but we still need updated's rationale (empty for
    // a pin, since NextBestActionSelector only writes a rationale for its own computed picks).
    setNextBestActionId(updated.id);
    setNextBestRationale(nextMoveRationaleText(updated));
  };

  return (
    <div className="space-y-4">
      {error ? (
        <p role="alert" className="text-sm text-merlot-600">
          {error}
        </p>
      ) : null}

      {isCreating ? (
        <ActionCreateForm onCreate={handleCreate} onCancel={() => setIsCreating(false)} />
      ) : (
        <Button onClick={() => setIsCreating(true)}>Add action</Button>
      )}

      {actions.length === 0 ? (
        <p className="text-sm text-ink-500">No actions yet. Add your first one above.</p>
      ) : (
        <ul className="space-y-3">
          {actions.map((action) => (
            <li key={action.id}>
              <ActionRow
                action={action}
                isNextBest={action.id === nextBestActionId}
                nextBestRationale={action.id === nextBestActionId ? nextBestRationale : null}
                onStatusChange={(status) => handleStatusChange(action.id, status)}
                onSetNextBest={() => handleSetNextBest(action.id)}
                isReflecting={action.id === reflectingActionId}
                onAddReflection={(whatHappened, learning) => handleAddReflection(action.id, whatHappened, learning)}
                onDismissReflection={() => setReflectingActionId(null)}
              />
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
