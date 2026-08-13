"use client";

import { useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { EditableTextCard } from "@/components/plan/editable-text-card";
import { apiMutate } from "@/lib/api-client";
import type { Plan, PlanDraft } from "@/lib/plan";

export function PlanView({ initialPlan }: { initialPlan: Plan | null }) {
  const [plan, setPlan] = useState(initialPlan);
  const [draft, setDraft] = useState<PlanDraft | null>(null);
  const [isLoadingDraft, setIsLoadingDraft] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  const loadDraft = async () => {
    setIsLoadingDraft(true);
    setError(null);
    const response = await fetch("/api/plan/draft");
    setIsLoadingDraft(false);
    if (!response.ok) {
      setError("We couldn't draft a plan just now. Please try again.");
      return;
    }
    setDraft((await response.json()) as PlanDraft);
  };

  const saveDraft = async () => {
    if (!draft) return;
    setIsSaving(true);
    setError(null);
    const response = await apiMutate("/api/plan", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(draft),
    });
    setIsSaving(false);
    if (!response.ok) {
      setError("We couldn't save your plan. Please try again.");
      return;
    }
    setPlan((await response.json()) as Plan);
    setDraft(null);
  };

  if (!plan && !draft) {
    return (
      <Card>
        <h2 className="font-display text-lg font-semibold text-ink-900">Draft your plan</h2>
        <p className="mt-1 text-sm text-ink-700">
          We&rsquo;ll turn your Dream Statement into a 5-year vision down to a 90-day mission,
          fully editable before you save anything.
        </p>
        {error ? (
          <p role="alert" className="mt-3 text-sm text-merlot-600">
            {error}
          </p>
        ) : null}
        <Button onClick={loadDraft} isLoading={isLoadingDraft} className="mt-4">
          Draft my plan
        </Button>
      </Card>
    );
  }

  if (draft && !plan) {
    return (
      <div className="space-y-6">
        <p className="text-sm text-ink-500">Edit anything below, then save your plan.</p>
        <DraftField
          label="5-Year Vision"
          value={draft.fiveYearVision}
          onChange={(v) => setDraft({ ...draft, fiveYearVision: v })}
        />
        <DraftField
          label="3-Year Direction"
          value={draft.threeYearDirection}
          onChange={(v) => setDraft({ ...draft, threeYearDirection: v })}
        />
        <DraftField
          label="1-Year Goal"
          value={draft.oneYearGoal}
          onChange={(v) => setDraft({ ...draft, oneYearGoal: v })}
        />
        <DraftField
          label="90-Day Mission"
          value={draft.ninetyDayMission}
          onChange={(v) => setDraft({ ...draft, ninetyDayMission: v })}
        />
        {error ? (
          <p role="alert" className="text-sm text-merlot-600">
            {error}
          </p>
        ) : null}
        <Button onClick={saveDraft} isLoading={isSaving}>
          Save my plan
        </Button>
      </div>
    );
  }

  if (!plan) return null;

  const fiveYear = plan.goals.find((g) => g.horizon === "fiveYear");
  const threeYear = plan.goals.find((g) => g.horizon === "threeYear");
  const oneYear = plan.goals.find((g) => g.horizon === "oneYear");
  const mission = plan.missions[0];

  const saveGoal = async (goalId: string, statement: string) => {
    const response = await apiMutate(`/api/plan/goals/${goalId}`, {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ statement, targetDate: null }),
    });
    if (!response.ok) throw new Error("Failed to save goal");
    const updated = await response.json();
    setPlan((p) =>
      p ? { ...p, goals: p.goals.map((g) => (g.id === goalId ? updated : g)) } : p,
    );
  };

  const saveMission = async (missionId: string, title: string) => {
    const response = await apiMutate(`/api/plan/missions/${missionId}`, {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ title, targetDate: mission?.targetDate ?? null }),
    });
    if (!response.ok) throw new Error("Failed to save mission");
    const updated = await response.json();
    setPlan((p) =>
      p ? { ...p, missions: p.missions.map((m) => (m.id === missionId ? updated : m)) } : p,
    );
  };

  return (
    <div className="space-y-6">
      {fiveYear ? (
        <EditableTextCard
          label="5-Year Vision"
          description="Where this dream has taken you."
          value={fiveYear.statement}
          onSave={(v) => saveGoal(fiveYear.id, v)}
        />
      ) : null}
      {threeYear ? (
        <EditableTextCard
          label="3-Year Direction"
          description="The track record you're building toward."
          value={threeYear.statement}
          onSave={(v) => saveGoal(threeYear.id, v)}
        />
      ) : null}
      {oneYear ? (
        <EditableTextCard
          label="1-Year Goal"
          description="What this dream looks like a year from now."
          value={oneYear.statement}
          onSave={(v) => saveGoal(oneYear.id, v)}
        />
      ) : null}
      {mission ? (
        <EditableTextCard
          label="90-Day Mission"
          description="What you're focused on right now."
          value={mission.title}
          onSave={(v) => saveMission(mission.id, v)}
        />
      ) : null}
    </div>
  );
}

function DraftField({
  label,
  value,
  onChange,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
}) {
  return (
    <div>
      <label className="mb-1.5 block text-sm font-medium text-ink-900">{label}</label>
      <textarea
        value={value}
        onChange={(e) => onChange(e.target.value)}
        rows={3}
        className="w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 py-2.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
      />
    </div>
  );
}
