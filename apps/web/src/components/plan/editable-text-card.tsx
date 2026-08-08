"use client";

import { useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export function EditableTextCard({
  label,
  description,
  value,
  onSave,
}: {
  label: string;
  description: string;
  value: string;
  onSave: (value: string) => Promise<void>;
}) {
  const [isEditing, setIsEditing] = useState(false);
  const [draft, setDraft] = useState(value);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSave = async () => {
    setIsSaving(true);
    setError(null);
    try {
      await onSave(draft);
      setIsEditing(false);
    } catch {
      setError("Couldn't save that. Please try again.");
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Card>
      <p className="text-xs font-semibold uppercase tracking-wide text-beacon-600">{label}</p>
      <p className="mt-1 text-xs text-ink-500">{description}</p>

      {isEditing ? (
        <div className="mt-3 space-y-3">
          <textarea
            value={draft}
            onChange={(e) => setDraft(e.target.value)}
            rows={3}
            className="w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 py-2.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
          />
          {error ? (
            <p role="alert" className="text-sm text-merlot-600">
              {error}
            </p>
          ) : null}
          <div className="flex gap-2">
            <Button onClick={handleSave} isLoading={isSaving} className="px-4 text-sm">
              Save
            </Button>
            <Button
              variant="ghost"
              onClick={() => {
                setDraft(value);
                setIsEditing(false);
              }}
              className="px-4 text-sm"
            >
              Cancel
            </Button>
          </div>
        </div>
      ) : (
        <div className="mt-3">
          <p className="text-ink-900">{value}</p>
          <button
            type="button"
            onClick={() => setIsEditing(true)}
            className="mt-2 text-sm font-medium text-beacon-600 hover:underline"
          >
            Edit
          </button>
        </div>
      )}
    </Card>
  );
}
