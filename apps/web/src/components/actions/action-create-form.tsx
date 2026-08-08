"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input, Label, FieldError } from "@/components/ui/field";
import { createActionSchema, type CreateActionInput } from "@/lib/validation";

const selectClasses =
  "min-h-11 w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2";

export function ActionCreateForm({
  onCreate,
  onCancel,
}: {
  onCreate: (values: CreateActionInput) => Promise<void>;
  onCancel: () => void;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateActionInput>({
    resolver: zodResolver(createActionSchema),
    defaultValues: { priority: "medium", difficulty: "medium", expectedImpact: "medium" },
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
        <Label htmlFor="action-title">Title</Label>
        <Input id="action-title" autoFocus {...register("title")} aria-invalid={!!errors.title} />
        <FieldError id="action-title-error">{errors.title?.message}</FieldError>
      </div>

      <div>
        <Label htmlFor="action-description">Description</Label>
        <textarea
          id="action-description"
          rows={2}
          className="w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 py-2.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
          {...register("description")}
        />
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div>
          <Label htmlFor="action-priority">Priority</Label>
          <select id="action-priority" className={selectClasses} {...register("priority")}>
            <option value="low">Low</option>
            <option value="medium">Medium</option>
            <option value="high">High</option>
          </select>
        </div>
        <div>
          <Label htmlFor="action-difficulty">Difficulty</Label>
          <select id="action-difficulty" className={selectClasses} {...register("difficulty")}>
            <option value="easy">Easy</option>
            <option value="medium">Medium</option>
            <option value="hard">Hard</option>
          </select>
        </div>
        <div>
          <Label htmlFor="action-impact">Impact</Label>
          <select id="action-impact" className={selectClasses} {...register("expectedImpact")}>
            <option value="low">Low</option>
            <option value="medium">Medium</option>
            <option value="high">High</option>
          </select>
        </div>
        <div>
          <Label htmlFor="action-minutes">Est. minutes</Label>
          <Input id="action-minutes" type="number" min={1} {...register("estimatedMinutes")} />
        </div>
      </div>

      <div>
        <Label htmlFor="action-due">Due date</Label>
        <Input id="action-due" type="date" {...register("dueDate")} />
      </div>

      <div className="flex gap-3">
        <Button type="submit" isLoading={isSubmitting}>
          Add action
        </Button>
        <Button type="button" variant="ghost" onClick={onCancel}>
          Cancel
        </Button>
      </div>
    </form>
  );
}
