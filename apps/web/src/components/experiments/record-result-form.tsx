"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Label, FieldError } from "@/components/ui/field";
import { recordExperimentResultSchema, type RecordExperimentResultInput } from "@/lib/validation";

const textareaClasses =
  "w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 py-2.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2";
const selectClasses =
  "min-h-11 w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2";

export function RecordResultForm({
  onRecord,
  onCancel,
}: {
  onRecord: (values: RecordExperimentResultInput) => Promise<void>;
  onCancel: () => void;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RecordExperimentResultInput>({
    resolver: zodResolver(recordExperimentResultSchema),
    defaultValues: { outcome: "validated" },
  });

  return (
    <form
      className="mt-4 space-y-4 rounded-2xl border border-ink-300 bg-paper-raised p-5"
      onSubmit={handleSubmit(async (values) => {
        await onRecord(values);
      })}
      noValidate
    >
      <div>
        <Label htmlFor="result-outcome">What happened?</Label>
        <select id="result-outcome" className={selectClasses} {...register("outcome")}>
          <option value="validated">Validated: the hypothesis held up</option>
          <option value="partiallyValidated">Partially validated: mixed signal</option>
          <option value="invalidated">Invalidated: the hypothesis didn&rsquo;t hold up</option>
        </select>
      </div>

      <div>
        <Label htmlFor="result-evidence">Evidence (optional)</Label>
        <textarea id="result-evidence" rows={2} className={textareaClasses} {...register("evidence")} />
      </div>

      <div>
        <Label htmlFor="result-learning">What did you learn?</Label>
        <textarea
          id="result-learning"
          rows={2}
          className={textareaClasses}
          aria-invalid={!!errors.learning}
          {...register("learning")}
        />
        <FieldError id="result-learning-error">{errors.learning?.message}</FieldError>
      </div>

      <div className="flex gap-3">
        <Button type="submit" isLoading={isSubmitting}>
          Save result
        </Button>
        <Button type="button" variant="ghost" onClick={onCancel}>
          Cancel
        </Button>
      </div>
    </form>
  );
}
