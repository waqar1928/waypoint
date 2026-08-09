"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { ExperimentCreateForm } from "@/components/experiments/experiment-create-form";
import { ExperimentRow } from "@/components/experiments/experiment-row";
import { apiMutate } from "@/lib/api-client";
import type { CreateExperimentInput, RecordExperimentResultInput } from "@/lib/validation";
import type { Experiment, ExperimentStatus } from "@/lib/experiments";

export function ExperimentsBoard({ initialExperiments }: { initialExperiments: Experiment[] }) {
  const [experiments, setExperiments] = useState(initialExperiments);
  const [isCreating, setIsCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleCreate = async (values: CreateExperimentInput) => {
    setError(null);
    const response = await apiMutate("/api/experiments", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(values),
    });

    if (!response.ok) {
      setError("We couldn't add that experiment. Please try again.");
      return;
    }

    const created = (await response.json()) as Experiment;
    setExperiments((current) => [created, ...current]);
    setIsCreating(false);
  };

  const handleStatusChange = async (experimentId: string, status: ExperimentStatus) => {
    const response = await apiMutate(`/api/experiments/${experimentId}/status`, {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ status }),
    });
    if (!response.ok) {
      setError("We couldn't update that experiment's status. Please try again.");
      return;
    }
    const updated = (await response.json()) as Experiment;
    setExperiments((current) => current.map((e) => (e.id === experimentId ? updated : e)));
  };

  const handleRecordResult = async (experimentId: string, values: RecordExperimentResultInput) => {
    const response = await apiMutate(`/api/experiments/${experimentId}/results`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(values),
    });
    if (!response.ok) {
      setError("We couldn't save that result. Please try again.");
      return;
    }
    const updated = (await response.json()) as Experiment;
    setExperiments((current) => current.map((e) => (e.id === experimentId ? updated : e)));
  };

  return (
    <div className="space-y-4">
      {error ? (
        <p role="alert" className="text-sm text-merlot-600">
          {error}
        </p>
      ) : null}

      {isCreating ? (
        <ExperimentCreateForm onCreate={handleCreate} onCancel={() => setIsCreating(false)} />
      ) : (
        <Button onClick={() => setIsCreating(true)}>Add experiment</Button>
      )}

      {experiments.length === 0 ? (
        <p className="text-sm text-ink-500">
          No experiments yet — add the smallest possible next step you can test.
        </p>
      ) : (
        <ul className="space-y-3">
          {experiments.map((experiment) => (
            <li key={experiment.id}>
              <ExperimentRow
                experiment={experiment}
                onStatusChange={(status) => handleStatusChange(experiment.id, status)}
                onRecordResult={(values) => handleRecordResult(experiment.id, values)}
              />
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
