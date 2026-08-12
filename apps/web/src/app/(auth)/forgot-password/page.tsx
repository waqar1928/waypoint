"use client";

import { useState } from "react";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input, Label, FieldError, useFieldIds } from "@/components/ui/field";
import { forgotPasswordSchema, type ForgotPasswordInput } from "@/lib/validation";
import { apiMutate } from "@/lib/api-client";

export default function ForgotPasswordPage() {
  const [submitted, setSubmitted] = useState(false);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ForgotPasswordInput>({ resolver: zodResolver(forgotPasswordSchema) });

  const email = useFieldIds("email");

  const onSubmit = async (values: ForgotPasswordInput) => {
    // The API deliberately returns the same 202 response whether or not this email has an
    // account (see ForgotPasswordCommandHandler's doc comment) — this page must not branch on the
    // response at all, or it would leak account existence through its own behavior even though the
    // API itself is careful not to.
    await apiMutate("/api/auth/forgot-password", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(values),
    });
    setSubmitted(true);
  };

  if (submitted) {
    return (
      <Card>
        <h1 className="font-display text-2xl font-semibold text-ink-900">Check your email</h1>
        <p className="mt-3 text-sm text-ink-700">
          If an account exists for that email address, we&apos;ve sent a link to reset your
          password. It may take a few minutes to arrive.
        </p>
        <p className="mt-6 text-center text-sm text-ink-700">
          <Link href="/login" className="font-medium text-beacon-600 hover:underline">
            Back to log in
          </Link>
        </p>
      </Card>
    );
  }

  return (
    <Card>
      <h1 className="font-display text-2xl font-semibold text-ink-900">Reset your password</h1>
      <p className="mt-1 text-sm text-ink-700">
        Enter your email and we&apos;ll send you a link to choose a new password.
      </p>

      <form className="mt-6 space-y-5" onSubmit={handleSubmit(onSubmit)} noValidate>
        <div>
          <Label htmlFor={email.inputId}>Email</Label>
          <Input
            id={email.inputId}
            type="email"
            autoComplete="email"
            aria-invalid={!!errors.email}
            aria-describedby={errors.email ? email.errorId : undefined}
            {...register("email")}
          />
          <FieldError id={email.errorId}>{errors.email?.message}</FieldError>
        </div>

        <Button type="submit" className="w-full" isLoading={isSubmitting}>
          Send reset link
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-ink-700">
        <Link href="/login" className="font-medium text-beacon-600 hover:underline">
          Back to log in
        </Link>
      </p>
    </Card>
  );
}
