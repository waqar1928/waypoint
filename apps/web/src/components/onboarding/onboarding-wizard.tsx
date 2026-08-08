"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { ArrowLeft, ArrowRight, Compass } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { ProgressBar } from "@/components/onboarding/progress-bar";
import { QuestionStep } from "@/components/onboarding/question-step";
import { DirectionCard } from "@/components/onboarding/direction-card";
import { DreamStatementForm } from "@/components/onboarding/dream-statement-form";
import { onboardingQuestions } from "@/lib/onboarding-questions";
import { apiMutate } from "@/lib/api-client";
import { isProblemDetails } from "@/lib/api-types";
import type { DiscoveryAnswers, DreamDirection } from "@/lib/dream";
import type { DreamStatementInput } from "@/lib/validation";

type Phase = "questions" | "generating" | "directions" | "statement";

export function OnboardingWizard() {
  const router = useRouter();
  const [phase, setPhase] = useState<Phase>("questions");
  const [questionIndex, setQuestionIndex] = useState(0);
  const [answers, setAnswers] = useState<DiscoveryAnswers>({});
  const [directions, setDirections] = useState<DreamDirection[]>([]);
  const [selectedDirection, setSelectedDirection] = useState<DreamDirection | null>(null);
  const [error, setError] = useState<string | null>(null);

  const question = onboardingQuestions[questionIndex];
  const isLastQuestion = questionIndex === onboardingQuestions.length - 1;

  const goBack = () => {
    if (phase === "statement") {
      setPhase("directions");
      return;
    }
    if (phase === "directions") {
      setPhase("questions");
      return;
    }
    setQuestionIndex((i) => Math.max(0, i - 1));
  };

  const advance = async () => {
    if (!isLastQuestion) {
      setQuestionIndex((i) => i + 1);
      return;
    }

    setPhase("generating");
    setError(null);
    try {
      const response = await apiMutate("/api/dreams/directions", {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify(answers),
      });
      if (!response.ok) {
        throw new Error("Failed to generate directions");
      }
      const result = (await response.json()) as DreamDirection[];
      setDirections(result);
      setPhase("directions");
    } catch {
      setError("We couldn't generate directions just now. Please try again.");
      setPhase("questions");
    }
  };

  const selectDirection = (direction: DreamDirection) => {
    setSelectedDirection(direction);
    setPhase("statement");
  };

  const submitDream = async (values: DreamStatementInput) => {
    setError(null);
    const response = await apiMutate("/api/dreams", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(values),
    });

    if (response.ok) {
      router.push("/app/dashboard");
      return;
    }

    const payload = await response.json().catch(() => null);
    setError(
      isProblemDetails(payload) && payload.detail
        ? payload.detail
        : "We couldn't save your dream. Please try again.",
    );
  };

  return (
    <div className="mx-auto max-w-2xl px-4 py-10 sm:px-6 sm:py-16">
      <div className="mb-8 flex items-center gap-2 font-display text-lg font-semibold text-ink-900">
        <Compass className="h-5 w-5 text-beacon-500" aria-hidden="true" />
        Waypoint
      </div>

      {phase === "questions" ? (
        <>
          <ProgressBar
            step={questionIndex + 1}
            total={onboardingQuestions.length}
            label={question.stage === 1 ? "Discover: Who Are You?" : "Discover Your Dream"}
          />
          <QuestionStep
            question={question}
            textValue={question.type === "text" ? (answers[question.key] as string | undefined) ?? "" : ""}
            chipValues={question.type === "chips" ? answers.feelings ?? [] : []}
            onTextChange={(value) =>
              question.type === "text" && setAnswers((a) => ({ ...a, [question.key]: value }))
            }
            onToggleChip={(option) =>
              setAnswers((a) => {
                const current = a.feelings ?? [];
                const next = current.includes(option)
                  ? current.filter((v) => v !== option)
                  : [...current, option];
                return { ...a, feelings: next };
              })
            }
          />
          {error ? (
            <p role="alert" className="mt-4 text-sm text-merlot-600">
              {error}
            </p>
          ) : null}
          <div className="mt-8 flex items-center justify-between">
            <Button variant="ghost" onClick={goBack} disabled={questionIndex === 0} className="gap-2">
              <ArrowLeft className="h-4 w-4" aria-hidden="true" />
              Back
            </Button>
            <div className="flex gap-3">
              <Button variant="secondary" onClick={advance}>
                Skip
              </Button>
              <Button onClick={advance} className="gap-2">
                {isLastQuestion ? "See my directions" : "Continue"}
                <ArrowRight className="h-4 w-4" aria-hidden="true" />
              </Button>
            </div>
          </div>
        </>
      ) : null}

      {phase === "generating" ? (
        <Card className="py-16 text-center">
          <p className="text-ink-700">Drafting some directions based on what you shared…</p>
        </Card>
      ) : null}

      {phase === "directions" ? (
        <div>
          <h1 className="font-display text-2xl font-semibold text-ink-900 sm:text-3xl">
            A few directions worth exploring
          </h1>
          <p className="mt-2 text-ink-700">
            These are drafts built from your own words — not answers. Pick one to start shaping, or go back and
            share more.
          </p>
          <div className="mt-8 grid gap-6 sm:grid-cols-2">
            {directions.map((direction, i) => (
              <DirectionCard key={i} direction={direction} onSelect={() => selectDirection(direction)} />
            ))}
          </div>
          <Button variant="ghost" onClick={goBack} className="mt-8 gap-2">
            <ArrowLeft className="h-4 w-4" aria-hidden="true" />
            Back to questions
          </Button>
        </div>
      ) : null}

      {phase === "statement" && selectedDirection ? (
        <div>
          <h1 className="font-display text-2xl font-semibold text-ink-900 sm:text-3xl">Make it yours</h1>
          <p className="mt-2 text-ink-700">
            Everything below is editable — change anything that doesn&rsquo;t sound like you.
          </p>
          <div className="mt-8">
            <DreamStatementForm
              initial={{
                title: selectedDirection.directionStatement.slice(0, 80),
                statement: selectedDirection.directionStatement,
                purpose: selectedDirection.whyItFits,
                whoItHelps: "",
                problem: "",
                outcome: "",
                motivation: "",
                impact: "",
                isBusinessShaped: selectedDirection.suggestedBusinessShaped,
              }}
              submitLabel="Save my dream"
              onSubmit={submitDream}
              errorMessage={error}
            />
          </div>
          <Button variant="ghost" onClick={goBack} className="mt-4 gap-2">
            <ArrowLeft className="h-4 w-4" aria-hidden="true" />
            Back to directions
          </Button>
        </div>
      ) : null}
    </div>
  );
}
