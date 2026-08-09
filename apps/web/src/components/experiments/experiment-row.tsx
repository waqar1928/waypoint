"use client";

import { useState } from "react";
import { clsx } from "clsx";
import { FlaskConical } from "lucide-react";
import { Card } from "@/components/ui/card";
import { RecordResultForm } from "@/components/experiments/record-result-form";
import type { Experiment, ExperimentOutcome, ExperimentStatus } from "@/lib/experiments";
import type { RecordExperimentResultInput } from "@/lib/validation";

const statusLabels: Record<ExperimentStatus, string> = {
  planned: "Planned",
  running: "Running",
  completed: "Completed",
};

const outcomeLabels: Record<ExperimentOutcome, string> = {
  validated: "Validated",
  partiallyValidated: "Partially validated",
  invalidated: "Invalidated",
};

const outcomeStyles: Record<ExperimentOutcome, string> = {
  validated: "bg-sage-100 text-sage-600",
  partiallyValidated: "bg-amber-100 text-amber-600",
  invalidated: "bg-merlot-100 text-merlot-600",
};

export function ExperimentRow({
  experiment,
  onStatusChange,
  onRecordResult,
}: {
  experiment: Experiment;
  onStatusChange: (status: ExperimentStatus) => void;
  onRecordResult: (values: RecordExperimentResultInput) => Promise<void>;
}) {
  const [isRecording, setIsRecording] = useState(false);

  return (
    <Card>
      <div className="flex items-start justify-between gap-4">
        <div className="min-w-0 flex-1">
          <div className="flex items-start gap-2">
            <FlaskConical className="mt-1 h-4 w-4 shrink-0 text-beacon-500" aria-hidden="true" />
            <h3 className="font-display text-base font-semibold text-ink-900">{experiment.ideaDescription}</h3>
          </div>
          <p className="mt-2 text-sm text-ink-700">
            <span className="font-medium text-ink-900">Hypothesis: </span>
            {experiment.hypothesis}
          </p>
          <p className="mt-1 text-sm text-ink-500">
            <span className="font-medium text-ink-700">Success looks like: </span>
            {experiment.successCriteria}
          </p>
        </div>

        <select
          aria-label={`Status for ${experiment.ideaDescription}`}
          value={experiment.status}
          onChange={(e) => onStatusChange(e.target.value as ExperimentStatus)}
          className="min-h-9 shrink-0 rounded-[10px] border border-ink-300 bg-paper-raised px-2 text-xs text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
        >
          {Object.entries(statusLabels).map(([value, label]) => (
            <option key={value} value={value}>
              {label}
            </option>
          ))}
        </select>
      </div>

      {experiment.results.length > 0 ? (
        <ul className="mt-4 space-y-3 border-t border-ink-300 pt-4">
          {experiment.results.map((result) => (
            <li key={result.id}>
              <span className={clsx("rounded-full px-2 py-0.5 text-xs font-medium", outcomeStyles[result.outcome])}>
                {outcomeLabels[result.outcome]}
              </span>
              <p className="mt-1.5 text-sm text-ink-900">
                <span className="font-medium">Learned: </span>
                {result.learning}
              </p>
              {result.evidence ? <p className="mt-1 text-sm text-ink-500">{result.evidence}</p> : null}
            </li>
          ))}
        </ul>
      ) : null}

      {isRecording ? (
        <RecordResultForm
          onRecord={async (values) => {
            await onRecordResult(values);
            setIsRecording(false);
          }}
          onCancel={() => setIsRecording(false)}
        />
      ) : (
        <button
          type="button"
          onClick={() => setIsRecording(true)}
          className="mt-4 text-xs font-medium text-beacon-600 hover:underline"
        >
          Record a result
        </button>
      )}
    </Card>
  );
}
