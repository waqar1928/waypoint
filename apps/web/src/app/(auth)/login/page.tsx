"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input, Label, FieldError, useFieldIds } from "@/components/ui/field";
import { loginSchema, type LoginInput } from "@/lib/validation";
import { isProblemDetails } from "@/lib/api-types";
import { apiMutate, invalidateCsrfToken } from "@/lib/api-client";

export default function LoginPage() {
  const router = useRouter();
  const [formError, setFormError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginInput>({ resolver: zodResolver(loginSchema) });

  const email = useFieldIds("email");
  const password = useFieldIds("password");

  const onSubmit = async (values: LoginInput) => {
    setFormError(null);
    const response = await apiMutate("/api/auth/login", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(values),
    });

    if (response.ok) {
      invalidateCsrfToken();
      const session = (await response.json()) as { onboardingCompleted: boolean };
      router.push(session.onboardingCompleted ? "/app/dashboard" : "/onboarding");
      return;
    }

    const payload = await response.json().catch(() => null);
    setFormError(
      isProblemDetails(payload) && payload.detail
        ? payload.detail
        : "That email and password don't match. Please try again.",
    );
  };

  return (
    <Card>
      <h1 className="font-display text-2xl font-semibold text-ink-900">Welcome back</h1>
      <p className="mt-1 text-sm text-ink-700">Log in to pick up where you left off.</p>

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

        <div>
          <div className="flex items-center justify-between">
            <Label htmlFor={password.inputId} className="mb-0">Password</Label>
            <Link href="/forgot-password" className="mb-1.5 text-sm text-beacon-600 hover:underline">
              Forgot password?
            </Link>
          </div>
          <Input
            id={password.inputId}
            type="password"
            autoComplete="current-password"
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
          Log in
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-ink-700">
        New to Waypoint?{" "}
        <Link href="/register" className="font-medium text-beacon-600 hover:underline">
          Find your dream
        </Link>
      </p>
    </Card>
  );
}
