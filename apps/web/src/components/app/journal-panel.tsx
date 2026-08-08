"use client";

import { useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { apiMutate } from "@/lib/api-client";
import type { JournalEntry, JournalEntryType } from "@/lib/journal";

const entryTypeLabels: Record<JournalEntryType, string> = {
  daily: "Daily reflection",
  weekly: "Weekly reflection",
  lesson: "Lesson",
  win: "Win",
  failure: "Failure",
  idea: "Idea",
  gratitude: "Gratitude",
  vision: "Future vision",
};

export function JournalPanel({ initialEntries }: { initialEntries: JournalEntry[] }) {
  const [entries, setEntries] = useState(initialEntries);
  const [entryType, setEntryType] = useState<JournalEntryType>("daily");
  const [body, setBody] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!body.trim()) return;

    setIsSubmitting(true);
    setError(null);
    const response = await apiMutate("/api/journal", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ entryType, body }),
    });
    setIsSubmitting(false);

    if (!response.ok) {
      setError("We couldn't save that entry. Please try again.");
      return;
    }

    const created = (await response.json()) as JournalEntry;
    setEntries((current) => [created, ...current]);
    setBody("");
  };

  return (
    <Card>
      <h2 className="font-display text-lg font-semibold text-ink-900">Journal</h2>
      <p className="mt-1 text-sm text-ink-500">A private space for reflection — only you can see this.</p>

      <form className="mt-4 space-y-3" onSubmit={handleSubmit}>
        <select
          value={entryType}
          onChange={(e) => setEntryType(e.target.value as JournalEntryType)}
          className="min-h-11 rounded-[10px] border border-ink-300 bg-paper-raised px-3 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
          aria-label="Entry type"
        >
          {Object.entries(entryTypeLabels).map(([value, label]) => (
            <option key={value} value={value}>
              {label}
            </option>
          ))}
        </select>
        <textarea
          value={body}
          onChange={(e) => setBody(e.target.value)}
          rows={3}
          placeholder="What's on your mind?"
          className="w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 py-2.5 text-sm text-ink-900 placeholder:text-ink-500 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
        />
        {error ? (
          <p role="alert" className="text-sm text-merlot-600">
            {error}
          </p>
        ) : null}
        <Button type="submit" isLoading={isSubmitting} className="w-full sm:w-auto">
          Add entry
        </Button>
      </form>

      {entries.length > 0 ? (
        <ul className="mt-6 space-y-4 border-t border-ink-300 pt-4">
          {entries.map((entry) => (
            <li key={entry.id} className="text-sm">
              <div className="flex items-center gap-2 text-ink-500">
                <span className="font-medium text-ink-700">{entryTypeLabels[entry.entryType]}</span>
                <span aria-hidden="true">&middot;</span>
                <time dateTime={entry.createdAt}>{new Date(entry.createdAt).toLocaleDateString("en-US")}</time>
              </div>
              <p className="mt-1 text-ink-900">{entry.body}</p>
            </li>
          ))}
        </ul>
      ) : (
        <p className="mt-6 border-t border-ink-300 pt-4 text-sm text-ink-500">No entries yet.</p>
      )}
    </Card>
  );
}
