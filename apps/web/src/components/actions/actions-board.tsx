"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { ActionCreateForm } from "@/components/actions/action-create-form";
import { ActionRow } from "@/components/actions/action-row";
import { apiMutate } from "@/lib/api-client";
import type { CreateActionInput } from "@/lib/validation";
import type { ActionStatus, WaypointAction } from "@/lib/actions";

export function ActionsBoard({ initialActions }: { initialActions: WaypointAction[] }) {
  const [actions, setActions] = useState(initialActions);
  const [isCreating, setIsCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);

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
                onStatusChange={(status) => handleStatusChange(action.id, status)}
                onSetNextBest={() => handleSetNextBest(action.id)}
              />
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
