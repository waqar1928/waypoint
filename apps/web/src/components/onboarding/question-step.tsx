"use client";

import { clsx } from "clsx";
import type { OnboardingQuestion } from "@/lib/onboarding-questions";

interface QuestionStepProps {
  question: OnboardingQuestion;
  textValue: string;
  chipValues: string[];
  onTextChange: (value: string) => void;
  onToggleChip: (option: string) => void;
}

export function QuestionStep({ question, textValue, chipValues, onTextChange, onToggleChip }: QuestionStepProps) {
  return (
    <div>
      <h2 className="font-display text-2xl font-semibold text-ink-900 sm:text-3xl">{question.prompt}</h2>

      {question.type === "text" ? (
        <textarea
          autoFocus
          rows={5}
          value={textValue}
          onChange={(e) => onTextChange(e.target.value)}
          placeholder={question.placeholder}
          className="mt-6 w-full rounded-[10px] border border-ink-300 bg-paper-raised px-4 py-3 text-base text-ink-900 placeholder:text-ink-500 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
        />
      ) : (
        <div className="mt-6 flex flex-wrap gap-2" role="group" aria-label={question.prompt}>
          {question.options.map((option) => {
            const selected = chipValues.includes(option);
            return (
              <button
                key={option}
                type="button"
                aria-pressed={selected}
                onClick={() => onToggleChip(option)}
                className={clsx(
                  "rounded-full border px-4 py-2 text-sm font-medium transition-colors focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2",
                  selected
                    ? "border-beacon-500 bg-beacon-500 text-white"
                    : "border-ink-300 bg-paper-raised text-ink-700 hover:border-ink-900",
                )}
              >
                {option}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}
