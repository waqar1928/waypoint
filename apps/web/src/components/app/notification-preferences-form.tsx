"use client";

import { useEffect, useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { notificationPreferencesSchema, type NotificationPreferencesInput } from "@/lib/validation";
import type { NotificationPreferences } from "@/lib/notification-preferences";
import { isProblemDetails } from "@/lib/api-types";
import { apiMutate } from "@/lib/api-client";
import {
  detectPushSupport,
  enablePushNotifications,
  disablePushNotificationsLocally,
  type PushSupport,
} from "@/lib/push-notifications";

const emailToggles: { name: keyof NotificationPreferencesInput; label: string; description: string }[] = [
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

const unsupportedReasonText: Record<string, string> = {
  "no-service-worker": "Push notifications aren't supported in this browser.",
  "no-push-manager": "Push notifications aren't supported in this browser.",
  "ios-not-installed":
    'On iPhone, add Drevia to your Home Screen first (Share, then "Add to Home Screen") to enable notifications.',
};

/** "HH:mm:ss" (the API's TimeOnly shape) -> "HH:mm" (what <input type="time"> wants), or "" for
 * null/unset. */
function toTimeInputValue(value: string | null): string {
  return value ? value.slice(0, 5) : "";
}

export function NotificationPreferencesForm({ preferences }: { preferences: NotificationPreferences }) {
  const [status, setStatus] = useState<"idle" | "saved" | "error">("idle");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [pushSupport, setPushSupport] = useState<PushSupport | null>(null);
  const [pushStatus, setPushStatus] = useState<"idle" | "working" | "error">("idle");
  const [pushErrorMessage, setPushErrorMessage] = useState<string | null>(null);
  const [subscriptionId, setSubscriptionId] = useState<string | null>(null);

  useEffect(() => {
    // Support detection touches navigator/window - must run client-side only, after mount. The
    // await here (not just an IIFE) is what satisfies the react-hooks/set-state-in-effect rule -
    // same pattern already used by notification-bell.tsx's mount-time poll.
    void (async () => {
      await Promise.resolve();
      setPushSupport(detectPushSupport());
    })();
  }, []);

  const {
    register,
    handleSubmit,
    control,
    setValue,
    getValues,
    formState: { isSubmitting },
  } = useForm<NotificationPreferencesInput>({
    resolver: zodResolver(notificationPreferencesSchema),
    defaultValues: {
      emailProductUpdates: preferences.emailProductUpdates,
      emailCoachNudges: preferences.emailCoachNudges,
      emailCommunityActivity: preferences.emailCommunityActivity,
      pushEnabled: preferences.pushEnabled,
      pushDetailedContent: preferences.pushDetailedContent,
      pushDailyReminderEnabled: preferences.pushDailyReminderEnabled,
      quietHoursStart: preferences.quietHoursStart,
      quietHoursEnd: preferences.quietHoursEnd,
    },
  });

  // useWatch (not the useForm().watch() function) so this component stays compatible with the
  // React Compiler's memoization - watch() returns a plain function reference React Compiler
  // can't safely reason about, useWatch is a proper hook.
  const pushEnabled = useWatch({ control, name: "pushEnabled" });
  const quietHoursStart = useWatch({ control, name: "quietHoursStart" });
  const quietHoursEnd = useWatch({ control, name: "quietHoursEnd" });

  const savePreferences = (values: NotificationPreferencesInput) =>
    apiMutate("/api/me/notification-preferences", {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(values),
    });

  /**
   * The one control in this form with an immediate side effect (a browser permission prompt and
   * a real subscribe/unsubscribe call) - saves right away rather than waiting for the "Save
   * preferences" button below, since leaving the browser subscribed/unsubscribed out of sync with
   * what the server thinks would be confusing. Everything else in this form (daily reminder,
   * detailed content, quiet hours) is a plain preference with no browser-permission implication,
   * so those stay on the normal Save-button flow together with the email toggles.
   */
  const handleTogglePush = async () => {
    setPushStatus("working");
    setPushErrorMessage(null);

    try {
      if (!pushEnabled) {
        const { subscriptionId: newId } = await enablePushNotifications();
        setSubscriptionId(newId);
        setValue("pushEnabled", true);
        const response = await savePreferences({ ...getValues(), pushEnabled: true });
        if (!response.ok) {
          throw new Error("We couldn't save that. Please try again.");
        }
      } else {
        await disablePushNotificationsLocally();
        if (subscriptionId) {
          await apiMutate(`/api/notifications/push-subscriptions/${subscriptionId}`, { method: "DELETE" });
        }
        setValue("pushEnabled", false);
        const response = await savePreferences({ ...getValues(), pushEnabled: false });
        if (!response.ok) {
          throw new Error("We couldn't save that. Please try again.");
        }
      }
      setPushStatus("idle");
    } catch (err) {
      setPushStatus("error");
      setPushErrorMessage(err instanceof Error ? err.message : "Something went wrong. Please try again.");
    }
  };

  const onSubmit = async (values: NotificationPreferencesInput) => {
    setStatus("idle");
    setErrorMessage(null);

    const response = await savePreferences(values);

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

      <div className="mt-6 rounded-[10px] border border-ink-300 p-4">
        <h3 className="font-display text-base font-semibold text-ink-900">Push notifications</h3>
        <p className="mt-1 text-sm text-ink-700">
          A quiet, occasional nudge when your next move is ready. Off by default - this is never turned
          on without you choosing to.
        </p>

        {pushSupport && !pushSupport.supported ? (
          <p className="mt-3 text-sm text-ink-500">
            {unsupportedReasonText[pushSupport.reason ?? ""] ??
              "Push notifications aren't supported on this device."}
          </p>
        ) : (
          <div className="mt-3">
            <Button
              type="button"
              variant={pushEnabled ? "secondary" : "primary"}
              isLoading={pushStatus === "working"}
              onClick={handleTogglePush}
            >
              {pushEnabled ? "Turn off push notifications" : "Enable push notifications"}
            </Button>
            {pushStatus === "error" && pushErrorMessage ? (
              <p role="alert" className="mt-2 text-sm text-merlot-600">
                {pushErrorMessage}
              </p>
            ) : null}
          </div>
        )}

        {pushEnabled ? (
          <div className="mt-5 space-y-4 border-t border-ink-300 pt-4">
            <label className="flex items-start gap-3 text-sm text-ink-700">
              <input
                type="checkbox"
                className="mt-0.5 h-4 w-4 rounded border-ink-300"
                {...register("pushDailyReminderEnabled")}
              />
              <span>
                <span className="block font-medium text-ink-900">Daily reminder</span>
                <span className="block text-ink-500">
                  One nudge a day, at most, when there&apos;s a next move ready.
                </span>
              </span>
            </label>

            <label className="flex items-start gap-3 text-sm text-ink-700">
              <input
                type="checkbox"
                className="mt-0.5 h-4 w-4 rounded border-ink-300"
                {...register("pushDetailedContent")}
              />
              <span>
                <span className="block font-medium text-ink-900">Show what the reminder is about</span>
                <span className="block text-ink-500">
                  Off by default: notifications just say &ldquo;Your next move is ready.&rdquo; Turn this
                  on to show the actual next action&apos;s title instead.
                </span>
              </span>
            </label>

            <div>
              <span className="block text-sm font-medium text-ink-900">Quiet hours</span>
              <p className="text-sm text-ink-500">Reminders are delayed until quiet hours end, never skipped.</p>
              <div className="mt-2 flex items-center gap-2">
                <input
                  type="time"
                  aria-label="Quiet hours start"
                  className="min-h-9 rounded-[10px] border border-ink-300 bg-paper-raised px-2 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
                  value={toTimeInputValue(quietHoursStart)}
                  onChange={(e) => setValue("quietHoursStart", e.target.value ? `${e.target.value}:00` : null)}
                />
                <span className="text-sm text-ink-500">to</span>
                <input
                  type="time"
                  aria-label="Quiet hours end"
                  className="min-h-9 rounded-[10px] border border-ink-300 bg-paper-raised px-2 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
                  value={toTimeInputValue(quietHoursEnd)}
                  onChange={(e) => setValue("quietHoursEnd", e.target.value ? `${e.target.value}:00` : null)}
                />
              </div>
            </div>
          </div>
        ) : null}
      </div>

      <p className="mt-6 text-sm text-ink-700">Choose what Drevia can email you about.</p>
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
        {emailToggles.map((toggle) => (
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
