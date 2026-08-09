"use client";

import { useId } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Label, FieldError } from "@/components/ui/field";
import { businessIdeaSchema, type BusinessIdeaInput } from "@/lib/validation";

interface FieldSpec {
  key: keyof BusinessIdeaInput;
  label: string;
  hint: string;
}

const sections: { heading: string; fields: FieldSpec[] }[] = [
  {
    heading: "The problem, and who it's for",
    fields: [
      { key: "problem", label: "Problem", hint: "What's broken or missing today." },
      { key: "customer", label: "Customer", hint: "Who specifically has this problem." },
      { key: "valueProposition", label: "Value proposition", hint: "Why they'd choose you over doing nothing." },
    ],
  },
  {
    heading: "How it works",
    fields: [
      { key: "solution", label: "Solution", hint: "What you'll actually build or offer." },
      { key: "businessModel", label: "Business model", hint: "How the pieces fit together to make this run." },
      { key: "technology", label: "Technology", hint: "What you need to build or use." },
    ],
  },
  {
    heading: "The market",
    fields: [
      { key: "market", label: "Market", hint: "How big is this, and who else is in it." },
      { key: "competitors", label: "Competitors", hint: "Who else solves this today, formally or informally." },
      { key: "pricing", label: "Pricing", hint: "What you'd charge, and why." },
    ],
  },
  {
    heading: "Getting to customers",
    fields: [
      { key: "marketing", label: "Marketing", hint: "How people will find out this exists." },
      { key: "sales", label: "Sales", hint: "How someone goes from interested to paying." },
      { key: "operations", label: "Operations", hint: "What it takes to actually deliver, day to day." },
    ],
  },
  {
    heading: "Money and risk",
    fields: [
      { key: "financialAssumptions", label: "Financial assumptions", hint: "Costs, margins, what has to be true to make money." },
      { key: "risks", label: "Risks", hint: "What could make this not work." },
    ],
  },
];

export function BusinessProfileForm({
  initial,
  onSave,
}: {
  initial: BusinessIdeaInput;
  onSave: (values: BusinessIdeaInput) => Promise<void>;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<BusinessIdeaInput>({
    resolver: zodResolver(businessIdeaSchema),
    defaultValues: initial,
  });

  const formId = useId();

  return (
    <form className="space-y-8" onSubmit={handleSubmit(onSave)} noValidate>
      {sections.map((section) => (
        <div key={section.heading} className="space-y-4">
          <h3 className="font-display text-base font-semibold text-ink-900">{section.heading}</h3>
          <div className="grid gap-4 sm:grid-cols-3">
            {section.fields.map((field) => {
              const inputId = `${formId}-${field.key}`;
              const errorId = `${formId}-${field.key}-error`;
              return (
                <div key={field.key}>
                  <Label htmlFor={inputId}>{field.label}</Label>
                  <p className="mb-1.5 -mt-1 text-xs text-ink-500">{field.hint}</p>
                  <textarea
                    id={inputId}
                    rows={3}
                    aria-invalid={!!errors[field.key]}
                    aria-describedby={errors[field.key] ? errorId : undefined}
                    className="w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 py-2.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
                    {...register(field.key)}
                  />
                  <FieldError id={errorId}>{errors[field.key]?.message as string | undefined}</FieldError>
                </div>
              );
            })}
          </div>
        </div>
      ))}

      <Button type="submit" isLoading={isSubmitting}>
        Save business profile
      </Button>
    </form>
  );
}
