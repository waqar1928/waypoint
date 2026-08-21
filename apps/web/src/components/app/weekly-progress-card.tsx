"use client";

import { useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { apiMutate } from "@/lib/api-client";
import type { JournalEntry } from "@/lib/journal";

const ONE_WEEK_MS = 7 * 24 * 60 * 60 * 1000;

/**
 * A lightweight weekly reflection prompt, built entirely on the existing Journal feature -
 * JournalEntryType.Weekly already existed and was already selectable in the general Journal
 * panel, it just had no dedicated surface of its own. This doesn't add a new domain concept: it
 * POSTs to the same /api/journal endpoint with entryType: "weekly" that JournalPanel already
 * uses, and reads from the same recent-entries list Dashboard already fetches (filtered to
 * weekly ones by the caller).
 */
export function WeeklyProgressCard({
  weeklyEntries,
  actionsCompleted,
  experimentsRun,
  learningsCount,
}: {
  /** Journal entries already filtered to entryType === "weekly", newest first. */
  weeklyEntries: JournalEntry[];
  actionsCompleted: number;
  experimentsRun: number;
  learningsCount: number;
}) {
  const [entries, setEntries] = useState(weeklyEntries);
  const [body, setBody] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  // Date.now() is impure and must not be called directly during render (see the
  // react-hooks/purity rule) - a lazy useState initializer runs exactly once per mount, which is
  // the accepted escape hatch for "what time did this component load" style reads.
  const [nowMs] = useState(() => Date.now());

  const latest = entries[0] ?? null;
  const hasReflectedThisWeek = latest ? nowMs - new Date(latest.createdAt).getTime() < ONE_WEEK_MS : false;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!body.trim()) return;

    setIsSubmitting(true);
    setError(null);
    const response = await apiMutate("/api/journal", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ entryType: "weekly", body }),
    });
    setIsSubmitting(false);

    if (!response.ok) {
      setError("We couldn't save that. Please try again.");
      return;
    }

    const created = (await response.json()) as JournalEntry;
    setEntries((current) => [created, ...current]);
    setBody("");
  };

  return (
    <Card id="weekly-progress" className="mt-6">
      <h2 className="font-display text-lg font-semibold text-ink-900">Weekly progress</h2>
      <p className="mt-1 text-sm text-ink-700">
        {actionsCompleted} {actionsCompleted === 1 ? "action" : "actions"} completed
        {" · "}
        {experimentsRun} {experimentsRun === 1 ? "experiment" : "experiments"} run
        {" · "}
        {learningsCount} {learningsCount === 1 ? "thing" : "things"} learned
      </p>

      {latest ? (
        <div className="mt-4 border-t border-ink-300 pt-4">
          <p className="text-xs font-medium text-ink-500">
            Last reflection ·{" "}
            <time dateTime={latest.createdAt}>{new Date(latest.createdAt).toLocaleDateString("en-US")}</time>
          </p>
          <p className="mt-1 text-sm text-ink-900">{latest.body}</p>
        </div>
      ) : null}

      {!hasReflectedThisWeek ? (
        <form className="mt-4 space-y-3 border-t border-ink-300 pt-4" onSubmit={handleSubmit}>
          <label className="block text-sm font-medium text-ink-900" htmlFor="weekly-reflection">
            How did this week go?
          </label>
          <textarea
            id="weekly-reflection"
            rows={3}
            value={body}
            onChange={(e) => setBody(e.target.value)}
            placeholder="What moved forward, what didn't, what you'd do differently."
            className="w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 py-2.5 text-sm text-ink-900 placeholder:text-ink-500 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
          />
          {error ? (
            <p role="alert" className="text-sm text-merlot-600">
              {error}
            </p>
          ) : null}
          <Button type="submit" isLoading={isSubmitting}>
            Save reflection
          </Button>
        </form>
      ) : (
        <p className="mt-4 border-t border-ink-300 pt-4 text-sm text-ink-500">
          You&apos;ve already reflected this week. Nice.
        </p>
      )}
    </Card>
  );
}
