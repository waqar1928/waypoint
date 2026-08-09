"use client";

import { useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import type { BusinessValidation } from "@/lib/business";

const toneClasses: Record<string, string> = {
  sage: "text-sage-600",
  amber: "text-amber-600",
  ink: "text-ink-500",
  beacon: "text-beacon-600",
};

export function ViabilityEstimatePanel({
  initialValidations,
  onGenerate,
}: {
  initialValidations: BusinessValidation[];
  onGenerate: () => Promise<BusinessValidation | null>;
}) {
  const [validations, setValidations] = useState(initialValidations);
  const [isGenerating, setIsGenerating] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const latest = validations[0] ?? null;
  const earlier = validations.slice(1);

  const handleGenerate = async () => {
    setIsGenerating(true);
    setError(null);
    const result = await onGenerate();
    setIsGenerating(false);
    if (!result) {
      setError("We couldn't generate an estimate. Make sure you've saved your business profile first.");
      return;
    }
    setValidations((current) => [result, ...current]);
  };

  return (
    <Card>
      <h2 className="font-display text-lg font-semibold text-ink-900">Dream Viability Estimate</h2>
      <p className="mt-1 text-sm text-ink-500">
        This is a decision-support estimate based on what you&rsquo;ve told us — not a guarantee of success.
      </p>

      <Button onClick={handleGenerate} isLoading={isGenerating} className="mt-4">
        Get viability estimate
      </Button>

      {error ? (
        <p role="alert" className="mt-2 text-sm text-merlot-600">
          {error}
        </p>
      ) : null}

      {latest ? (
        <div className="mt-6 space-y-4 border-t border-ink-300 pt-6">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-beacon-600">Viability</p>
            <p className="mt-1 font-display text-3xl font-semibold text-ink-900">
              {latest.viabilityEstimate ?? "—"}
              <span className="text-base font-normal text-ink-500">/100</span>
            </p>
          </div>

          <EstimateList title="What's working" items={latest.strongAssumptions} tone="sage" />
          <EstimateList title="Weak spots" items={latest.weakAssumptions} tone="amber" />
          <EstimateList title="Still unknown" items={latest.unknowns} tone="ink" />
          <EstimateList title="Try next" items={latest.recommendedExperiments} tone="beacon" />
        </div>
      ) : null}

      {earlier.length > 0 ? (
        <details className="mt-6 border-t border-ink-300 pt-4">
          <summary className="cursor-pointer text-sm font-medium text-beacon-600">
            {earlier.length} earlier {earlier.length === 1 ? "estimate" : "estimates"}
          </summary>
          <ul className="mt-3 space-y-1.5 text-sm text-ink-500">
            {earlier.map((v) => (
              <li key={v.id}>
                {new Date(v.createdAt).toLocaleDateString("en-US")} — {v.viabilityEstimate ?? "—"}/100
              </li>
            ))}
          </ul>
        </details>
      ) : null}
    </Card>
  );
}

function EstimateList({
  title,
  items,
  tone,
}: {
  title: string;
  items: string[];
  tone: "sage" | "amber" | "ink" | "beacon";
}) {
  if (items.length === 0) return null;
  return (
    <div>
      <p className={`text-xs font-semibold uppercase tracking-wide ${toneClasses[tone]}`}>{title}</p>
      <ul className="mt-1.5 space-y-1 text-sm text-ink-700">
        {items.map((item, i) => (
          <li key={i}>{item}</li>
        ))}
      </ul>
    </div>
  );
}
