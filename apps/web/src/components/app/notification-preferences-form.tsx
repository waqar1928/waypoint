"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { notificationPreferencesSchema, type NotificationPreferencesInput } from "@/lib/validation";
import type { NotificationPreferences } from "@/lib/notification-preferences";
import { isProblemDetails } from "@/lib/api-types";
import { apiMutate } from "@/lib/api-client";

const toggles: { name: keyof NotificationPreferencesInput; label: string; description: string }[] = [
  {
    name: "emailProductUpdates",
    label: "Product updates",
    description: "New features and changes to Drevia.",
  },
  {
    name: "emailCoachNudges",
    label: "Coach nudges",
    description: "Occasional reminders to check in with Drevia Coach.",
  },
  {
    name: "emailCommunityActivity",
    label: "Community activity",
    description: "Comments on your posts, responses to your help requests.",
  },
];

export function NotificationPreferencesForm({ preferences }: { preferences: NotificationPreferences }) {
  const [status, setStatus] = useState<"idle" | "saved" | "error">("idle");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { isSubmitting },
  } = useForm<NotificationPreferencesInput>({
    resolver: zodResolver(notificationPreferencesSchema),
    defaultValues: {
      emailProductUpdates: preferences.emailProductUpdates,
      emailCoachNudges: preferences.emailCoachNudges,
      emailCommunityActivity: preferences.emailCommunityActivity,
    },
  });

  const onSubmit = async (values: NotificationPreferencesInput) => {
    setStatus("idle");
    setErrorMessage(null);

    const response = await apiMutate("/api/me/notification-preferences", {
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
        : "We couldn't save your notification preferences. Please try again.",
    );
  };

  return (
    <section className="mt-10 border-t border-ink-300 pt-8">
      <h2 className="font-display text-xl font-semibold text-ink-900">Notifications</h2>
      <p className="mt-1 text-sm text-ink-700">Choose what Drevia can email you about.</p>
      {/* Honest, not a placeholder disclaimer to hide behind - these preferences save correctly,
          but nothing in the app checks them yet before sending an email. Saying so plainly here
          matches how this app handles every other not-fully-wired-up setting (see the Community
          post-visibility selector's "same as Community for now" note) rather than implying a
          promise the code doesn't keep. */}
      <p className="mt-1 text-xs text-ink-500">
        These choices are saved, but Drevia doesn&apos;t check them yet before sending an email. You
        may still receive email regardless of what you pick here until that&apos;s built.
      </p>

      <form className="mt-6 space-y-4" onSubmit={handleSubmit(onSubmit)}>
        {toggles.map((toggle) => (
          <label key={toggle.name} className="flex items-start gap-3 text-sm text-ink-700">
            <input
              type="checkbox"
              className="mt-0.5 h-4 w-4 rounded border-ink-300"
              {...register(toggle.name)}
            />
            <span>
              <span className="block font-medium text-ink-900">{toggle.label}</span>
              <span className="block text-ink-500">{toggle.description}</span>
            </span>
          </label>
        ))}

        {status === "saved" ? (
          <p role="status" className="text-sm text-sage-600">
            Notification preferences saved.
          </p>
        ) : null}
        {status === "error" ? (
          <p role="alert" className="text-sm text-merlot-600">
            {errorMessage}
          </p>
        ) : null}

        <Button type="submit" isLoading={isSubmitting}>
          Save preferences
        </Button>
      </form>
    </section>
  );
}
