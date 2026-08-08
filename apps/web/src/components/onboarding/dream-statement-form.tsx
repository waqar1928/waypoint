"use client";

import { useId } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input, Label, FieldError } from "@/components/ui/field";
import { dreamStatementSchema, type DreamStatementInput } from "@/lib/validation";

const fields: { key: keyof DreamStatementInput; label: string; hint: string; rows: number }[] = [
  { key: "statement", label: "Dream statement", hint: "One clear sentence of what you're building or becoming.", rows: 3 },
  { key: "purpose", label: "Purpose", hint: "Why this matters to you personally.", rows: 3 },
  { key: "whoItHelps", label: "Who it helps", hint: "The specific person or group.", rows: 2 },
  { key: "problem", label: "Problem being solved", hint: "What's broken or missing today.", rows: 3 },
  { key: "outcome", label: "Desired outcome", hint: "What success looks like.", rows: 2 },
  { key: "motivation", label: "Personal motivation", hint: "Your \"why,\" in your own words.", rows: 3 },
  { key: "impact", label: "Impact", hint: "The ripple effect beyond the immediate outcome.", rows: 2 },
];

export function DreamStatementForm({
  initial,
  submitLabel,
  onSubmit,
  errorMessage,
}: {
  initial: DreamStatementInput;
  submitLabel: string;
  onSubmit: (values: DreamStatementInput) => Promise<void>;
  errorMessage: string | null;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<DreamStatementInput>({
    resolver: zodResolver(dreamStatementSchema),
    defaultValues: initial,
  });

  const formId = useId();
  const titleInputId = `${formId}-title`;
  const titleErrorId = `${formId}-title-error`;

  return (
    <form className="space-y-6" onSubmit={handleSubmit(onSubmit)} noValidate>
      <div>
        <Label htmlFor={titleInputId}>Title</Label>
        <Input
          id={titleInputId}
          aria-invalid={!!errors.title}
          aria-describedby={errors.title ? titleErrorId : undefined}
          {...register("title")}
        />
        <FieldError id={titleErrorId}>{errors.title?.message}</FieldError>
      </div>

      {fields.map((field) => {
        const inputId = `${formId}-${field.key}`;
        const errorId = `${formId}-${field.key}-error`;
        return (
          <div key={field.key}>
            <Label htmlFor={inputId}>{field.label}</Label>
            <p className="mb-1.5 -mt-1 text-xs text-ink-500">{field.hint}</p>
            <textarea
              id={inputId}
              rows={field.rows}
              aria-invalid={!!errors[field.key]}
              aria-describedby={errors[field.key] ? errorId : undefined}
              className="w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 py-2.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
              {...register(field.key)}
            />
            <FieldError id={errorId}>{errors[field.key]?.message as string | undefined}</FieldError>
          </div>
        );
      })}

      <label className="flex items-center gap-2 text-sm text-ink-700">
        <input type="checkbox" className="h-4 w-4 rounded border-ink-300" {...register("isBusinessShaped")} />
        This dream is shaped like a business
      </label>

      {errorMessage ? (
        <p role="alert" className="text-sm text-merlot-600">
          {errorMessage}
        </p>
      ) : null}

      <Button type="submit" isLoading={isSubmitting}>
        {submitLabel}
      </Button>
    </form>
  );
}
