"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Label, useFieldIds } from "@/components/ui/field";
import { privacySettingsSchema, type PrivacySettingsInput, type VisibilityLevel } from "@/lib/validation";
import type { PrivacySettings } from "@/lib/privacy-settings";
import { isProblemDetails } from "@/lib/api-types";
import { apiMutate } from "@/lib/api-client";

const visibilityLabels: Record<VisibilityLevel, string> = {
  private: "Only me",
  followers: "Followers",
  community: "Drevia community",
  public: "Public",
};

export function PrivacySettingsForm({ settings }: { settings: PrivacySettings }) {
  const [status, setStatus] = useState<"idle" | "saved" | "error">("idle");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { isSubmitting },
  } = useForm<PrivacySettingsInput>({
    resolver: zodResolver(privacySettingsSchema),
    defaultValues: {
      profileVisibility: settings.profileVisibility,
      dreamVisibility: settings.dreamVisibility,
    },
  });

  const profileVisibility = useFieldIds("profileVisibility");
  const dreamVisibility = useFieldIds("dreamVisibility");

  const onSubmit = async (values: PrivacySettingsInput) => {
    setStatus("idle");
    setErrorMessage(null);

    const response = await apiMutate("/api/me/privacy-settings", {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(values),
    });

    if (response.ok) {
      setStatus("saved");
      return;
    }

    const payload = await response.json().catch(() => null);
    setStatus("error");
    setErrorMessage(
      isProblemDetails(payload) && payload.detail
        ? payload.detail
        : "We couldn't save your privacy settings. Please try again.",
    );
  };

  return (
    <section className="mt-10 border-t border-ink-300 pt-8">
      <h2 className="font-display text-xl font-semibold text-ink-900">Privacy</h2>
      <p className="mt-1 text-sm text-ink-700">Choose who can see your profile and your Dream.</p>
      {/* Same honesty standard as the notification-preferences disclaimer above: verified in the
          backend that nothing outside the Users module currently reads ProfileVisibility or
          DreamVisibility - no other feature checks these before showing your data to someone
          else. Also true: "Followers" is a real option here but there's no follow/social-graph
          system anywhere in Drevia yet (same gap the Community module's own code documents for
          its old 4-tier design), so picking it has no different effect from any other option
          today. Saying this plainly rather than letting the UI imply enforcement that doesn't
          exist yet. */}
      <p className="mt-1 text-xs text-ink-500">
        These choices are saved, but nothing in Drevia checks them yet before showing your profile
        or Dream to someone else. There&apos;s also no followers feature yet, so &ldquo;Followers&rdquo;
        doesn&apos;t currently behave differently from the other options. Treat this as setting a
        preference for later, not something enforced today.
      </p>

      <form className="mt-6 space-y-5" onSubmit={handleSubmit(onSubmit)}>
        <div>
          <Label htmlFor={profileVisibility.inputId}>Profile visibility</Label>
          <select
            id={profileVisibility.inputId}
            className="min-h-11 w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
            {...register("profileVisibility")}
          >
            {Object.entries(visibilityLabels).map(([value, label]) => (
              <option key={value} value={value}>
                {label}
              </option>
            ))}
          </select>
        </div>

        <div>
          <Label htmlFor={dreamVisibility.inputId}>Dream visibility</Label>
          <select
            id={dreamVisibility.inputId}
            className="min-h-11 w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
            {...register("dreamVisibility")}
          >
            {Object.entries(visibilityLabels).map(([value, label]) => (
              <option key={value} value={value}>
                {label}
              </option>
            ))}
          </select>
        </div>

        {status === "saved" ? (
          <p role="status" className="text-sm text-sage-600">
            Privacy settings saved.
          </p>
        ) : null}
        {status === "error" ? (
          <p role="alert" className="text-sm text-merlot-600">
            {errorMessage}
          </p>
        ) : null}

        <Button type="submit" isLoading={isSubmitting}>
          Save privacy settings
        </Button>
      </form>
    </section>
  );
}
