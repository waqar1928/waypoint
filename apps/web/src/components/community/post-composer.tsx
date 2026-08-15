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
    defaultValues: { visibility: "community", attachDream: false },
  });

  return (
    <form
      className="space-y-3 rounded-2xl border border-ink-300 bg-paper-raised p-5"
      onSubmit={handleSubmit(async (values) => {
        await onCreate(values);
        // attachDream resets to false (not preserved like visibility) - deliberately not sticky,
        // so it can't silently stay checked and attach your Dream to a post you didn't mean to.
        reset({ body: "", visibility: values.visibility, attachDream: false });
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

      {/* Opt-in, off by default, decided fresh per post - see createPostSchema.attachDream. The
          server resolves your own current Dream; there's no way for this checkbox to attach
          anyone else's. Shown to whoever the post's visibility allows, not just you. */}
      <label className="flex items-start gap-2 text-xs text-ink-500">
        <input type="checkbox" className="mt-0.5 h-3.5 w-3.5 rounded border-ink-300" {...register("attachDream")} />
        <span>Attach my Dream so people can see what this is about</span>
      </label>

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
            <option value="community">Drevia community</option>
            {/* Not actually "coming soon" — the backend already handles Public, it just isn't
                distinct from Community yet since there's no unauthenticated/external viewing
                surface (see ICommunityRepository's doc comment). The old label claimed a broken
                feature; this one describes what Public actually does today without overclaiming
                it means "visible outside Drevia," which it doesn't yet. */}
            <option value="public">Public (visible to all members, same as Community for now)</option>
          </select>
        </div>
        <Button type="submit" isLoading={isSubmitting}>
          Post
        </Button>
      </div>
    </form>
  );
}
