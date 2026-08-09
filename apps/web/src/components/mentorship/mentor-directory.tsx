"use client";

import { useState } from "react";
import { Card } from "@/components/ui/card";
import type { MentorProfile } from "@/lib/mentorship";

export function MentorDirectory({ initialMentors }: { initialMentors: MentorProfile[] }) {
  const [mentors, setMentors] = useState(initialMentors);
  const [filter, setFilter] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const search = async (value: string) => {
    setFilter(value);
    setIsLoading(true);
    const response = await fetch(`/api/mentorship/mentors${value ? `?expertise=${encodeURIComponent(value)}` : ""}`);
    setIsLoading(false);
    if (response.ok) {
      setMentors((await response.json()) as MentorProfile[]);
    }
  };

  return (
    <Card>
      <h2 className="font-display text-lg font-semibold text-ink-900">Mentor directory</h2>
      <p className="mt-1 text-sm text-ink-700">Browse people who&rsquo;ve opted in to help.</p>

      <input
        value={filter}
        onChange={(e) => search(e.target.value)}
        placeholder="Filter by expertise (e.g. marketing)"
        aria-label="Filter mentors by expertise"
        className="mt-3 min-h-11 w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
      />

      {isLoading ? (
        <p className="mt-3 text-sm text-ink-500">Searching…</p>
      ) : mentors.length === 0 ? (
        <p className="mt-3 text-sm text-ink-500">No mentors match yet.</p>
      ) : (
        <ul className="mt-3 space-y-3">
          {mentors.map((mentor) => (
            <li key={mentor.id} className="rounded-[10px] border border-ink-300 p-3">
              <div className="flex items-center justify-between gap-2">
                <p className="text-sm font-medium text-ink-900">{mentor.mentor.displayName}</p>
                {mentor.verificationStatus === "verified" ? (
                  <span className="rounded-full bg-sage-100 px-2 py-0.5 text-xs font-medium text-sage-600">Verified</span>
                ) : null}
              </div>
              <p className="mt-1 text-xs text-ink-500">
                {mentor.expertise.join(" · ")}
                {mentor.yearsExperience ? ` · ${mentor.yearsExperience} yrs` : ""}
                {mentor.availability ? ` · ${mentor.availability}` : ""}
              </p>
            </li>
          ))}
        </ul>
      )}
    </Card>
  );
}
