"use client";

import { useState } from "react";
import Link from "next/link";
import { Compass, ListChecks, FlaskConical, BookOpen, ArrowRight, Pencil } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Button, buttonClasses } from "@/components/ui/button";
import { DreamEditForm } from "@/components/app/dream-edit-form";
import type { Dream, DreamStage } from "@/lib/dream";
import type { Mission } from "@/lib/plan";
import type { WaypointAction } from "@/lib/actions";
import type { Experiment } from "@/lib/experiments";
import type { JournalEntry } from "@/lib/journal";

const stageLabels: Record<DreamStage, string> = {
  discover: "Discover",
  define: "Define",
  validate: "Validate",
  plan: "Plan",
  act: "Act",
  learn: "Learn",
  grow: "Grow",
};

export function DreamOverview({
  dream,
  currentMission,
  nextBestAction,
  activeExperiment,
  recentLearnings,
}: {
  dream: Dream;
  currentMission: Mission | null;
  nextBestAction: WaypointAction | null;
  activeExperiment: Experiment | null;
  recentLearnings: JournalEntry[];
}) {
  const [isEditing, setIsEditing] = useState(false);

  return (
    <>
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <div className="flex items-center gap-2">
            <Compass className="h-5 w-5 text-beacon-500" aria-hidden="true" />
            <span className="rounded-full bg-ink-100 px-2.5 py-0.5 text-xs font-medium text-ink-700">
              {stageLabels[dream.stage]}
            </span>
          </div>
          <h1 className="mt-2 font-display text-3xl font-semibold text-ink-900">{dream.title}</h1>
        </div>
        <div className="flex shrink-0 gap-2">
          <Link href="/app/coach?start=dreamAnalysis" className={buttonClasses("secondary")}>
            Ask Coach about this
          </Link>
          <Button variant="ghost" onClick={() => setIsEditing((v) => !v)} className="gap-2">
            <Pencil className="h-4 w-4" aria-hidden="true" />
            {isEditing ? "Done" : "Edit"}
          </Button>
        </div>
      </div>

      {isEditing ? (
        <div className="mt-8">
          <DreamEditForm dream={dream} onSaved={() => setIsEditing(false)} />
        </div>
      ) : (
        <div className="mt-8 space-y-6">
          <Card>
            <p className="text-ink-900">{dream.statement}</p>
            {dream.purpose ? (
              <p className="mt-3 text-sm text-ink-700">
                <span className="font-medium text-ink-900">Why it matters: </span>
                {dream.purpose}
              </p>
            ) : null}
            {dream.whoItHelps ? (
              <p className="mt-2 text-sm text-ink-700">
                <span className="font-medium text-ink-900">Who it helps: </span>
                {dream.whoItHelps}
              </p>
            ) : null}
            {dream.problem ? (
              <p className="mt-2 text-sm text-ink-700">
                <span className="font-medium text-ink-900">The problem: </span>
                {dream.problem}
              </p>
            ) : null}
            {dream.outcome ? (
              <p className="mt-2 text-sm text-ink-700">
                <span className="font-medium text-ink-900">What success looks like: </span>
                {dream.outcome}
              </p>
            ) : null}
          </Card>

          <Card>
            <ListChecks className="h-5 w-5 text-beacon-500" aria-hidden="true" />
            <h2 className="mt-3 font-display text-base font-semibold text-ink-900">Current focus</h2>
            {currentMission ? (
              <>
                <p className="mt-1 text-sm text-ink-700">{currentMission.title}</p>
                <Link href="/app/plan" className="mt-3 inline-block text-sm font-medium text-beacon-600 hover:underline">
                  Go to Plan
                </Link>
              </>
            ) : (
              <>
                <p className="mt-1 text-sm text-ink-500">You haven&apos;t set a 90-day mission yet.</p>
                <Link href="/app/plan" className="mt-3 inline-block text-sm font-medium text-beacon-600 hover:underline">
                  Draft your plan
                </Link>
              </>
            )}
          </Card>

          <Card className="border-beacon-500/40 bg-beacon-100/40">
            <p className="text-xs font-semibold uppercase tracking-wide text-beacon-600">Next move</p>
            {nextBestAction ? (
              <>
                <h2 className="mt-2 font-display text-lg font-semibold text-ink-900">{nextBestAction.title}</h2>
                {nextBestAction.rationale ? (
                  <p className="mt-1 text-sm text-beacon-700">{nextBestAction.rationale}</p>
                ) : null}
                <Link href="/app/actions" className={buttonClasses("primary", "mt-4 gap-2")}>
                  Go to Actions
                  <ArrowRight className="h-4 w-4" aria-hidden="true" />
                </Link>
              </>
            ) : (
              <>
                <h2 className="mt-2 font-display text-lg font-semibold text-ink-900">Add your first action</h2>
                <p className="mt-1 text-sm text-ink-700">A small, concrete next step you can start on.</p>
                <Link href="/app/actions" className={buttonClasses("primary", "mt-4 gap-2")}>
                  Go to Actions
                  <ArrowRight className="h-4 w-4" aria-hidden="true" />
                </Link>
              </>
            )}
          </Card>

          <Card>
            <FlaskConical className="h-5 w-5 text-beacon-500" aria-hidden="true" />
            <h2 className="mt-3 font-display text-base font-semibold text-ink-900">Active experiment</h2>
            {activeExperiment ? (
              <>
                <p className="mt-1 text-sm text-ink-700">{activeExperiment.ideaDescription}</p>
                <p className="mt-1 text-xs text-ink-500">{activeExperiment.hypothesis}</p>
                <Link href="/app/experiments" className="mt-3 inline-block text-sm font-medium text-beacon-600 hover:underline">
                  Go to Experiment Lab
                </Link>
              </>
            ) : (
              <>
                <p className="mt-1 text-sm text-ink-500">Test the smallest possible next step before you commit to it.</p>
                <Link href="/app/experiments" className="mt-3 inline-block text-sm font-medium text-beacon-600 hover:underline">
                  Start an experiment
                </Link>
              </>
            )}
          </Card>

          <Card>
            <BookOpen className="h-5 w-5 text-beacon-500" aria-hidden="true" />
            <h2 className="mt-3 font-display text-base font-semibold text-ink-900">What you&apos;ve learned</h2>
            {recentLearnings.length > 0 ? (
              <ul className="mt-3 space-y-3">
                {recentLearnings.map((entry) => (
                  <li key={entry.id} className="border-t border-ink-300 pt-3 first:border-t-0 first:pt-0">
                    <p className="text-sm text-ink-700">{entry.body}</p>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="mt-1 text-sm text-ink-500">
                Learnings show up here automatically once you record an experiment result or reflect on a completed action.
              </p>
            )}
          </Card>
        </div>
      )}
    </>
  );
}
