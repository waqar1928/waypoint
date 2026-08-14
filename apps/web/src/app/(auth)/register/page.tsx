"use client";

import { useState } from "react";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input, Label, FieldError, useFieldIds } from "@/components/ui/field";
import { registerSchema, type RegisterInput } from "@/lib/validation";
import { isProblemDetails } from "@/lib/api-types";
import { apiMutate } from "@/lib/api-client";

export default function RegisterPage() {
  const [formError, setFormError] = useState<string | null>(null);
  const [registered, setRegistered] = useState(false);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<RegisterInput>({ resolver: zodResolver(registerSchema) });

  const name = useFieldIds("displayName");
  const email = useFieldIds("email");
  const password = useFieldIds("password");

  const onSubmit = async (values: RegisterInput) => {
    setFormError(null);
    const response = await apiMutate("/api/auth/register", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(values),
    });

    if (response.ok) {
      // Registration creates the account and sends a confirmation email, but does not sign the
      // user in (RequireConfirmedAccount = true means login is blocked until they click the
      // email link, see DependencyInjection.cs) — redirecting straight to /app/dashboard here
      // would just bounce them to /login with no explanation, since there's no session yet. Show
      // a "check your email" state instead, matching forgot-password/page.tsx's pattern.
      setRegistered(true);
      return;
    }

    const payload = await response.json().catch(() => null);
    if (isProblemDetails(payload) && payload.errors) {
      for (const [field, messages] of Object.entries(payload.errors)) {
        setError(field as keyof RegisterInput, { message: messages[0] });
      }
      return;
    }
    setFormError(
      isProblemDetails(payload) && payload.detail
        ? payload.detail
        : "We couldn't create your account. Please try again.",
    );
  };

  if (registered) {
    return (
      <Card>
        <h1 className="font-display text-2xl font-semibold text-ink-900">Check your email</h1>
        <p className="mt-3 text-sm text-ink-700">
          We&apos;ve sent a confirmation link to your email address. Click it to activate your
          account, then log in.
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
      <h1 className="font-display text-2xl font-semibold text-ink-900">Find your dream</h1>
      <p className="mt-1 text-sm text-ink-700">Create your Drevia account to get started.</p>

      <form className="mt-6 space-y-5" onSubmit={handleSubmit(onSubmit)} noValidate>
        <div>
          <Label htmlFor={name.inputId}>Name</Label>
          <Input
            id={name.inputId}
            autoComplete="name"
            aria-invalid={!!errors.displayName}
            aria-describedby={errors.displayName ? name.errorId : undefined}
            {...register("displayName")}
          />
          <FieldError id={name.errorId}>{errors.displayName?.message}</FieldError>
        </div>

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

        <div>
          <Label htmlFor={password.inputId}>Password</Label>
          <Input
            id={password.inputId}
            type="password"
            autoComplete="new-password"
            aria-invalid={!!errors.password}
            aria-describedby={errors.password ? password.errorId : undefined}
            {...register("password")}
          />
          <FieldError id={password.errorId}>{errors.password?.message}</FieldError>
        </div>

        {formError ? (
          <p role="alert" className="text-sm text-merlot-600">
            {formError}
          </p>
        ) : null}

        <Button type="submit" className="w-full" isLoading={isSubmitting}>
          Create account
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-ink-700">
        Already have an account?{" "}
        <Link href="/login" className="font-medium text-beacon-600 hover:underline">
          Log in
        </Link>
      </p>
    </Card>
  );
}
