"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";

/**
 * The optional "what happened / what did you learn" mini-form shown right after an action is
 * marked completed. Shared by ActionRow (the Actions list) and NextMoveCard (Dream Overview's
 * interactive Next Move card) - extracted here so completing an action asks the same question the
 * same way regardless of which screen it happened on, instead of two copies drifting apart.
 */
export function ActionReflectionPrompt({
  actionId,
  onSave,
  onSkip,
}: {
  actionId: string;
  onSave: (whatHappened: string, learning: string) => void;
  onSkip: () => void;
}) {
  const [whatHappened, setWhatHappened] = useState("");
  const [learning, setLearning] = useState("");

  return (
    <div className="mt-4 space-y-3 border-t border-ink-300 pt-4">
      <div>
        <label className="text-xs font-medium text-ink-700" htmlFor={`what-happened-${actionId}`}>
          What happened? (optional)
        </label>
        <textarea
          id={`what-happened-${actionId}`}
          rows={2}
          value={whatHappened}
          onChange={(e) => setWhatHappened(e.target.value)}
          className="mt-1 w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3 py-2 text-sm text-ink-900"
        />
      </div>
      <div>
        <label className="text-xs font-medium text-ink-700" htmlFor={`learned-${actionId}`}>
          What did you learn? (optional)
        </label>
        <textarea
          id={`learned-${actionId}`}
          rows={2}
          value={learning}
          onChange={(e) => setLearning(e.target.value)}
          className="mt-1 w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3 py-2 text-sm text-ink-900"
        />
      </div>
      <div className="flex gap-3">
        <Button
          type="button"
          className="px-3 py-1.5 text-xs"
          onClick={() => onSave(whatHappened.trim(), learning.trim())}
          disabled={!whatHappened.trim() && !learning.trim()}
        >
          Save
        </Button>
        <button type="button" onClick={onSkip} className="text-xs font-medium text-ink-500 hover:text-ink-900">
          Skip
        </button>
      </div>
    </div>
  );
}
