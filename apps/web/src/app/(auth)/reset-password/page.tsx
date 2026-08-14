"use client";

import { Suspense, useState } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input, Label, FieldError, useFieldIds } from "@/components/ui/field";
import { resetPasswordSchema, type ResetPasswordInput } from "@/lib/validation";
import { isProblemDetails } from "@/lib/api-types";
import { apiMutate } from "@/lib/api-client";

function ResetPasswordForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const userId = searchParams.get("userId");
  const token = searchParams.get("token");

  const [formError, setFormError] = useState<string | null>(null);
  const [done, setDone] = useState(false);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordInput>({ resolver: zodResolver(resetPasswordSchema) });

  const newPassword = useFieldIds("newPassword");

  if (!userId || !token) {
    return (
      <Card>
        <h1 className="font-display text-2xl font-semibold text-ink-900">Invalid reset link</h1>
        <p className="mt-3 text-sm text-ink-700">
          This password reset link is missing information. Request a new one below.
        </p>
        <p className="mt-6 text-center text-sm text-ink-700">
          <Link href="/forgot-password" className="font-medium text-beacon-600 hover:underline">
            Request a new link
          </Link>
        </p>
      </Card>
    );
  }

  if (done) {
    return (
      <Card>
        <h1 className="font-display text-2xl font-semibold text-ink-900">Password updated</h1>
        <p className="mt-3 text-sm text-ink-700">
          Your password has been reset. You can log in with your new password now.
        </p>
        <Button className="mt-6 w-full" onClick={() => router.push("/login")}>
          Go to log in
        </Button>
      </Card>
    );
  }

  const onSubmit = async (values: ResetPasswordInput) => {
    setFormError(null);
    const response = await apiMutate("/api/auth/reset-password", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ userId, token, newPassword: values.newPassword }),
    });

    if (response.ok) {
      setDone(true);
      return;
    }

    const payload = await response.json().catch(() => null);
    setFormError(
      isProblemDetails(payload) && payload.detail
        ? payload.detail
        : "This reset link is invalid or has expired. Request a new one.",
    );
  };

  return (
    <Card>
      <h1 className="font-display text-2xl font-semibold text-ink-900">Choose a new password</h1>

      {/* method="post" is a defense-in-depth backstop against a native fallback submission (if
          hydration ever failed) defaulting to method="get" and putting the password in the URL —
          see the identical comment on register/page.tsx for the full explanation. */}
      <form className="mt-6 space-y-5" method="post" onSubmit={handleSubmit(onSubmit)} noValidate>
        <div>
          <Label htmlFor={newPassword.inputId}>New password</Label>
          <Input
            id={newPassword.inputId}
            type="password"
            autoComplete="new-password"
            aria-invalid={!!errors.newPassword}
            aria-describedby={errors.newPassword ? newPassword.errorId : undefined}
            {...register("newPassword")}
          />
          <FieldError id={newPassword.errorId}>{errors.newPassword?.message}</FieldError>
        </div>

        {formError ? (
          <p role="alert" className="text-sm text-merlot-600">
            {formError}
          </p>
        ) : null}

        <Button type="submit" className="w-full" isLoading={isSubmitting}>
          Reset password
        </Button>
      </form>
    </Card>
  );
}

export default function ResetPasswordPage() {
  // useSearchParams() requires a Suspense boundary in the App Router — this page is only ever
  // reached via a link with query params already attached, so there's nothing meaningful to show
  // during the (effectively instant) suspend, but Next.js still requires the boundary.
  return (
    <Suspense fallback={null}>
      <ResetPasswordForm />
    </Suspense>
  );
}
