"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Label, FieldError } from "@/components/ui/field";
import { createHelpRequestSchema, type CreateHelpRequestInput } from "@/lib/validation";

const categories: CreateHelpRequestInput["category"][] = [
  "business", "marketing", "technology", "finance", "sales", "design", "career", "operations", "leadership",
];

export function HelpRequestCreateForm({
  onCreate,
  onCancel,
}: {
  onCreate: (values: CreateHelpRequestInput) => Promise<void>;
  onCancel: () => void;
}) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateHelpRequestInput>({
    resolver: zodResolver(createHelpRequestSchema),
    defaultValues: { category: "business" },
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
        <Label htmlFor="help-category">Category</Label>
        <select
          id="help-category"
          className="min-h-11 w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3 text-sm text-ink-900"
          {...register("category")}
        >
          {categories.map((c) => (
            <option key={c} value={c}>
              {c.charAt(0).toUpperCase() + c.slice(1)}
            </option>
          ))}
        </select>
      </div>

      <div>
        <Label htmlFor="help-title">Title</Label>
        <input
          id="help-title"
          className="min-h-11 w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 text-sm text-ink-900"
          aria-invalid={!!errors.title}
          {...register("title")}
        />
        <FieldError id="help-title-error">{errors.title?.message}</FieldError>
      </div>

      <div>
        <Label htmlFor="help-body">What do you need help with?</Label>
        <textarea
          id="help-body"
          rows={3}
          className="w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 py-2.5 text-sm text-ink-900"
          aria-invalid={!!errors.body}
          {...register("body")}
        />
        <FieldError id="help-body-error">{errors.body?.message}</FieldError>
      </div>

      <div className="flex gap-3">
        <Button type="submit" isLoading={isSubmitting}>
          Post help request
        </Button>
        <Button type="button" variant="ghost" onClick={onCancel}>
          Cancel
        </Button>
      </div>
    </form>
  );
}
