"use client";

import { useState } from "react";
import { Trophy } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/field";
import { apiMutate } from "@/lib/api-client";
import type { Milestone } from "@/lib/plan";

export function MilestonesPanel({ initialMilestones }: { initialMilestones: Milestone[] }) {
  const [milestones, setMilestones] = useState(initialMilestones);
  const [title, setTitle] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) return;

    setIsSubmitting(true);
    setError(null);
    const response = await apiMutate("/api/milestones", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ title }),
    });
    setIsSubmitting(false);

    if (!response.ok) {
      setError("We couldn't add that milestone. Please try again.");
      return;
    }

    const created = (await response.json()) as Milestone;
    setMilestones((current) => [created, ...current]);
    setTitle("");
  };

  const handleAchieve = async (id: string) => {
    const response = await apiMutate(`/api/milestones/${id}/achieve`, { method: "POST" });
    if (!response.ok) return;
    const updated = (await response.json()) as Milestone;
    setMilestones((current) => current.map((m) => (m.id === id ? updated : m)));
  };

  return (
    // id="milestones" is the target of the sidebar's Milestones nav link (nav-items.ts points at
    // /app/dashboard#milestones). Same situation Journal was in: a real, working panel with no
    // nav entry at all until now.
    <Card id="milestones">
      <Trophy className="h-5 w-5 text-ink-500" aria-hidden="true" />
      <h2 className="mt-3 font-display text-base font-semibold text-ink-900">Milestones</h2>
      <p className="mt-1 text-sm text-ink-500">Mark the moments worth remembering.</p>

      <form className="mt-4 flex gap-2" onSubmit={handleSubmit}>
        <Input
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="e.g. Talked to my first customer"
          aria-label="New milestone"
        />
        <Button type="submit" isLoading={isSubmitting} className="shrink-0 px-4">
          Add
        </Button>
      </form>
      {error ? (
        <p role="alert" className="mt-2 text-sm text-merlot-600">
          {error}
        </p>
      ) : null}

      {milestones.length > 0 ? (
        <ul className="mt-4 space-y-2 border-t border-ink-300 pt-4">
          {milestones.map((m) => (
            <li key={m.id} className="flex items-center justify-between gap-3 text-sm">
              <span className={m.achievedAt ? "text-ink-500 line-through" : "text-ink-900"}>{m.title}</span>
              {!m.achievedAt ? (
                <button
                  type="button"
                  onClick={() => handleAchieve(m.id)}
                  className="shrink-0 text-xs font-medium text-beacon-600 hover:underline"
                >
                  Mark achieved
                </button>
              ) : null}
            </li>
          ))}
        </ul>
      ) : (
        <p className="mt-4 border-t border-ink-300 pt-4 text-sm text-ink-500">No milestones yet.</p>
      )}
    </Card>
  );
}
