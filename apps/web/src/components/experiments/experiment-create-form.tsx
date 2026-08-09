"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Label, FieldError } from "@/components/ui/field";
import { createExperimentSchema, type CreateExperimentInput } from "@/lib/validation";

const textareaClasses =
  "w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 py-2.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2";

export function ExperimentCreateForm({
  onCreate,
  onCancel,
}: {
  onCreate: (values: CreateExperimentInput) => Promise<void>;
  onCancel: () => void;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateExperimentInput>({
    resolver: zodResolver(createExperimentSchema),
  });

  return (
    <form
      className="space-y-4 rounded-2xl border border-ink-300 bg-paper-raised p-5"
      onSubmit={handleSubmit(async (values) => {
        await onCreate(values);
      })}
      noValidate
    >
      <div>
        <Label htmlFor="experiment-idea">What do you want to try?</Label>
        <textarea
          id="experiment-idea"
          rows={2}
          className={textareaClasses}
          aria-invalid={!!errors.ideaDescription}
          {...register("ideaDescription")}
        />
        <FieldError id="experiment-idea-error">{errors.ideaDescription?.message}</FieldError>
      </div>

      <div>
        <Label htmlFor="experiment-hypothesis">What do you expect to happen?</Label>
        <textarea
          id="experiment-hypothesis"
          rows={2}
          className={textareaClasses}
          aria-invalid={!!errors.hypothesis}
          {...register("hypothesis")}
        />
        <FieldError id="experiment-hypothesis-error">{errors.hypothesis?.message}</FieldError>
      </div>

      <div>
        <Label htmlFor="experiment-success">How will you know it worked?</Label>
        <textarea
          id="experiment-success"
          rows={2}
          className={textareaClasses}
          aria-invalid={!!errors.successCriteria}
          {...register("successCriteria")}
        />
        <FieldError id="experiment-success-error">{errors.successCriteria?.message}</FieldError>
      </div>

      <div className="flex gap-3">
        <Button type="submit" isLoading={isSubmitting}>
          Add experiment
        </Button>
        <Button type="button" variant="ghost" onClick={onCancel}>
          Cancel
        </Button>
      </div>
    </form>
  );
}
