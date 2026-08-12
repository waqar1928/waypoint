"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { isProblemDetails } from "@/lib/api-types";
import { apiMutate, invalidateCsrfToken } from "@/lib/api-client";

/**
 * Found missing during the production-readiness pass's live verification (see
 * docs/PRODUCTION_READINESS_AUDIT.md) — the backend's DeleteAccountCommand has existed and been
 * tested since Phase 9, but no frontend route ever called it. That same live-verification pass
 * also found account deletion didn't cascade to any other module's data (Dream, journal, goals,
 * community posts, etc. were all left behind, orphaned) — that gap has since been closed with a
 * dedicated cascade-deletion pass, verified for real by seeding data across every module, calling
 * this exact delete flow, and confirming every table was empty afterward. Deletion is now
 * genuinely complete and immediate.
 */
export function DeleteAccountSection() {
  const router = useRouter();
  const [isConfirming, setIsConfirming] = useState(false);
  const [password, setPassword] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleDelete = async () => {
    setErrorMessage(null);
    setIsSubmitting(true);

    const response = await apiMutate("/api/me", {
      method: "DELETE",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ password }),
    });

    if (response.ok) {
      invalidateCsrfToken();
      router.push("/");
      router.refresh();
      return;
    }

    setIsSubmitting(false);
    const payload = await response.json().catch(() => null);
    setErrorMessage(
      isProblemDetails(payload) && payload.detail
        ? payload.detail
        : "We couldn't delete your account. Please try again.",
    );
  };

  return (
    <div className="mt-12 rounded-2xl border border-merlot-300 bg-merlot-50 p-6">
      <h2 className="font-display text-lg font-semibold text-merlot-700">Delete account</h2>
      <p className="mt-2 text-sm text-merlot-700">
        Permanently deletes your account and everything in it — your login, profile, Dream,
        journal, goals, actions, experiments, business plans, AI conversations, community posts
        and comments, and mentorship activity. This can&apos;t be undone.
      </p>

      {!isConfirming ? (
        <button
          type="button"
          onClick={() => setIsConfirming(true)}
          className="mt-4 inline-flex min-h-11 items-center justify-center rounded-[10px] border border-merlot-600 px-5 text-sm font-medium text-merlot-700 transition-colors hover:bg-merlot-100 focus-visible:outline-2 focus-visible:outline-merlot-600 focus-visible:outline-offset-2"
        >
          Delete my account
        </button>
      ) : (
        <div className="mt-4 space-y-3">
          <label htmlFor="delete-account-password" className="block text-sm font-medium text-merlot-700">
            Confirm your password to permanently delete your account
          </label>
          <input
            id="delete-account-password"
            type="password"
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="w-full max-w-sm rounded-[10px] border border-merlot-300 bg-paper-raised px-3.5 py-2.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-merlot-600 focus-visible:outline-offset-2"
          />
          {errorMessage ? (
            <p role="alert" className="text-sm text-merlot-700">
              {errorMessage}
            </p>
          ) : null}
          <div className="flex gap-3">
            <button
              type="button"
              onClick={handleDelete}
              disabled={!password || isSubmitting}
              className="inline-flex min-h-11 items-center justify-center rounded-[10px] bg-merlot-600 px-5 text-sm font-medium text-white transition-colors hover:bg-merlot-700 disabled:cursor-not-allowed disabled:bg-ink-300 disabled:text-ink-500"
            >
              {isSubmitting ? "…" : "Permanently delete"}
            </button>
            <button
              type="button"
              onClick={() => {
                setIsConfirming(false);
                setPassword("");
                setErrorMessage(null);
              }}
              className="inline-flex min-h-11 items-center justify-center rounded-[10px] px-5 text-sm font-medium text-ink-700 hover:bg-ink-100"
            >
              Cancel
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
