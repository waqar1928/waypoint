"use client";

import { useState } from "react";
import { BusinessProfileForm } from "@/components/business/business-profile-form";
import { ViabilityEstimatePanel } from "@/components/business/viability-estimate-panel";
import { apiMutate } from "@/lib/api-client";
import type { BusinessIdea, BusinessValidation } from "@/lib/business";
import type { BusinessIdeaInput } from "@/lib/validation";

function toFormValues(idea: BusinessIdea | null): BusinessIdeaInput {
  return {
    problem: idea?.problem ?? "",
    customer: idea?.customer ?? "",
    valueProposition: idea?.valueProposition ?? "",
    solution: idea?.solution ?? "",
    businessModel: idea?.businessModel ?? "",
    market: idea?.market ?? "",
    competitors: idea?.competitors ?? "",
    pricing: idea?.pricing ?? "",
    marketing: idea?.marketing ?? "",
    sales: idea?.sales ?? "",
    operations: idea?.operations ?? "",
    technology: idea?.technology ?? "",
    financialAssumptions: idea?.financialAssumptions ?? "",
    risks: idea?.risks ?? "",
  };
}

export function BusinessBuilderWorkspace({
  initialIdea,
  initialValidations,
}: {
  initialIdea: BusinessIdea | null;
  initialValidations: BusinessValidation[];
}) {
  const [idea, setIdea] = useState(initialIdea);
  const [error, setError] = useState<string | null>(null);

  const handleSave = async (values: BusinessIdeaInput) => {
    setError(null);
    const response = await apiMutate("/api/business-idea", {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(values),
    });
    if (!response.ok) {
      setError("We couldn't save your business profile. Please try again.");
      return;
    }
    setIdea((await response.json()) as BusinessIdea);
  };

  const handleGenerate = async (): Promise<BusinessValidation | null> => {
    const response = await apiMutate("/api/business-idea/validations", { method: "POST" });
    if (!response.ok) return null;
    return (await response.json()) as BusinessValidation;
  };

  return (
    <div className="space-y-8">
      {error ? (
        <p role="alert" className="text-sm text-merlot-600">
          {error}
        </p>
      ) : null}
      <BusinessProfileForm initial={toFormValues(idea)} onSave={handleSave} />
      <ViabilityEstimatePanel initialValidations={initialValidations} onGenerate={handleGenerate} />
    </div>
  );
}
