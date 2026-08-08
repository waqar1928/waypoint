"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { DreamStatementForm } from "@/components/onboarding/dream-statement-form";
import { apiMutate } from "@/lib/api-client";
import { isProblemDetails } from "@/lib/api-types";
import type { Dream } from "@/lib/dream";
import type { DreamStatementInput } from "@/lib/validation";

export function DreamEditForm({ dream }: { dream: Dream }) {
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);

  const handleSubmit = async (values: DreamStatementInput) => {
    setError(null);
    setSaved(false);
    const response = await apiMutate("/api/dreams/me", {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(values),
    });

    if (response.ok) {
      setSaved(true);
      router.refresh();
      return;
    }

    const payload = await response.json().catch(() => null);
    setError(
      isProblemDetails(payload) && payload.detail
        ? payload.detail
        : "We couldn't save your dream. Please try again.",
    );
  };

  return (
    <>
      {saved ? (
        <p role="status" className="mb-4 text-sm text-sage-600">
          Saved.
        </p>
      ) : null}
      <DreamStatementForm
        initial={{
          title: dream.title,
          statement: dream.statement,
          purpose: dream.purpose ?? "",
          whoItHelps: dream.whoItHelps ?? "",
          problem: dream.problem ?? "",
          outcome: dream.outcome ?? "",
          motivation: dream.motivation ?? "",
          impact: dream.impact ?? "",
          isBusinessShaped: dream.isBusinessShaped,
        }}
        submitLabel="Save changes"
        onSubmit={handleSubmit}
        errorMessage={error}
      />
    </>
  );
}
