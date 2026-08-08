"use client";

import { useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input, Label, FieldError, useFieldIds } from "@/components/ui/field";
import { profileSchema, type ProfileInput } from "@/lib/validation";
import type { Profile } from "@/lib/profile";
import { isProblemDetails } from "@/lib/api-types";
import { apiMutate } from "@/lib/api-client";

export function ProfileForm({ profile }: { profile: Profile }) {
  const [status, setStatus] = useState<"idle" | "saved" | "error">("idle");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<ProfileInput>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      displayName: profile.displayName,
      bio: profile.bio ?? "",
      timeZone: profile.timeZone,
    },
  });

  const timeZones = useMemo(() => {
    if (typeof Intl.supportedValuesOf === "function") {
      return Intl.supportedValuesOf("timeZone");
    }
    return [profile.timeZone];
  }, [profile.timeZone]);

  const name = useFieldIds("displayName");
  const bio = useFieldIds("bio");
  const timeZone = useFieldIds("timeZone");

  const onSubmit = async (values: ProfileInput) => {
    setStatus("idle");
    setErrorMessage(null);

    const response = await apiMutate("/api/me/profile", {
      method: "PUT",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(values),
    });

    if (response.ok) {
      setStatus("saved");
      return;
    }

    const payload = await response.json().catch(() => null);
    if (isProblemDetails(payload) && payload.errors) {
      for (const [field, messages] of Object.entries(payload.errors)) {
        setError(field as keyof ProfileInput, { message: messages[0] });
      }
      return;
    }
    setStatus("error");
    setErrorMessage(
      isProblemDetails(payload) && payload.detail
        ? payload.detail
        : "We couldn't save your profile. Please try again.",
    );
  };

  return (
    <form className="mt-8 space-y-6" onSubmit={handleSubmit(onSubmit)} noValidate>
      <div>
        <Label htmlFor={name.inputId}>Name</Label>
        <Input
          id={name.inputId}
          aria-invalid={!!errors.displayName}
          aria-describedby={errors.displayName ? name.errorId : undefined}
          {...register("displayName")}
        />
        <FieldError id={name.errorId}>{errors.displayName?.message}</FieldError>
      </div>

      <div>
        <Label htmlFor={bio.inputId}>Bio</Label>
        <textarea
          id={bio.inputId}
          rows={4}
          aria-invalid={!!errors.bio}
          aria-describedby={errors.bio ? bio.errorId : undefined}
          className="w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 py-2.5 text-sm text-ink-900 placeholder:text-ink-500 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
          placeholder="A few sentences about where you're at right now."
          {...register("bio")}
        />
        <FieldError id={bio.errorId}>{errors.bio?.message}</FieldError>
      </div>

      <div>
        <Label htmlFor={timeZone.inputId}>Time zone</Label>
        <select
          id={timeZone.inputId}
          aria-invalid={!!errors.timeZone}
          aria-describedby={errors.timeZone ? timeZone.errorId : undefined}
          className="min-h-11 w-full rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
          {...register("timeZone")}
        >
          {timeZones.map((tz) => (
            <option key={tz} value={tz}>
              {tz}
            </option>
          ))}
        </select>
        <FieldError id={timeZone.errorId}>{errors.timeZone?.message}</FieldError>
      </div>

      {status === "saved" ? (
        <p role="status" className="text-sm text-sage-600">
          Profile saved.
        </p>
      ) : null}
      {status === "error" ? (
        <p role="alert" className="text-sm text-merlot-600">
          {errorMessage}
        </p>
      ) : null}

      <Button type="submit" isLoading={isSubmitting}>
        Save profile
      </Button>
    </form>
  );
}
