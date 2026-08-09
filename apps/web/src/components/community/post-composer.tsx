"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Label, FieldError } from "@/components/ui/field";
import { createPostSchema, type CreatePostInput } from "@/lib/validation";

export function PostComposer({ onCreate }: { onCreate: (values: CreatePostInput) => Promise<void> }) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<CreatePostInput>({
    resolver: zodResolver(createPostSchema),
    defaultValues: { visibility: "community" },
  });

  return (
    <form
      className="space-y-3 rounded-2xl border border-ink-300 bg-paper-raised p-5"
      onSubmit={handleSubmit(async (values) => {
        await onCreate(values);
        reset({ body: "", visibility: values.visibility });
      })}
      noValidate
    >
      <div>
        <Label htmlFor="post-body">Share something with the community</Label>
        <textarea
          id="post-body"
          rows={3}
          placeholder="What's on your mind?"
          className="w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 py-2.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
          aria-invalid={!!errors.body}
          {...register("body")}
        />
        <FieldError id="post-body-error">{errors.body?.message}</FieldError>
      </div>

      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <Label htmlFor="post-visibility" className="mb-0">
            Visible to
          </Label>
          <select
            id="post-visibility"
            className="min-h-9 rounded-[10px] border border-ink-300 bg-paper-raised px-2 text-sm text-ink-900"
            {...register("visibility")}
          >
            <option value="private">Only me</option>
            <option value="community">Waypoint community</option>
            <option value="public">Public (coming soon)</option>
          </select>
        </div>
        <Button type="submit" isLoading={isSubmitting}>
          Post
        </Button>
      </div>
    </form>
  );
}
