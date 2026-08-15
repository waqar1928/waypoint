"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { HelpRequestCreateForm } from "@/components/mentorship/help-request-create-form";
import { HelpRequestCard } from "@/components/mentorship/help-request-card";
import { apiMutate } from "@/lib/api-client";
import type { HelpRequest } from "@/lib/mentorship";
import type { CreateHelpRequestInput } from "@/lib/validation";

export function HelpRequestsBoard({ initialRequests }: { initialRequests: HelpRequest[] }) {
  const [requests, setRequests] = useState(initialRequests);
  const [isCreating, setIsCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleCreate = async (values: CreateHelpRequestInput) => {
    setError(null);
    const response = await apiMutate("/api/mentorship/help-requests", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(values),
    });
    if (!response.ok) {
      setError("We couldn't post that. Please try again.");
      return;
    }
    const created = (await response.json()) as HelpRequest;
    setRequests((current) => [created, ...current]);
    setIsCreating(false);
  };

  const handleUpdate = (updated: HelpRequest) => {
    setRequests((current) => current.map((r) => (r.id === updated.id ? updated : r)));
  };

  return (
    <div className="space-y-4">
      {error ? (
        <p role="alert" className="text-sm text-merlot-600">
          {error}
        </p>
      ) : null}

      {isCreating ? (
        <HelpRequestCreateForm onCreate={handleCreate} onCancel={() => setIsCreating(false)} />
      ) : (
        <Button onClick={() => setIsCreating(true)}>Ask for help</Button>
      )}

      {requests.length === 0 ? (
        <p className="text-sm text-ink-500">No help requests yet. Be the first to ask.</p>
      ) : (
        <ul className="space-y-3">
          {requests.map((r) => (
            <li key={r.id}>
              <HelpRequestCard request={r} onUpdate={handleUpdate} />
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
