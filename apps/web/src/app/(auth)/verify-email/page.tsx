"use client";

import { Suspense, useEffect, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { Card } from "@/components/ui/card";
import { buttonClasses } from "@/components/ui/button";
import { isProblemDetails } from "@/lib/api-types";
import { apiMutate } from "@/lib/api-client";

type VerifyState = "verifying" | "success" | "error" | "missing-params";

function VerifyEmailContent() {
  const searchParams = useSearchParams();
  const userId = searchParams.get("userId");
  const token = searchParams.get("token");
  const [state, setState] = useState<VerifyState>(userId && token ? "verifying" : "missing-params");
  const [errorDetail, setErrorDetail] = useState<string | null>(null);

  useEffect(() => {
    if (!userId || !token) return;

    let cancelled = false;
    (async () => {
      const response = await apiMutate("/api/auth/verify-email", {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify({ userId, token }),
      });

      if (cancelled) return;

      if (response.ok) {
        setState("success");
        return;
      }

      const payload = await response.json().catch(() => null);
      setErrorDetail(isProblemDetails(payload) && payload.detail ? payload.detail : null);
      setState("error");
    })();

    return () => {
      cancelled = true;
    };
    // Deliberately runs once on mount only — userId/token come from the URL this page was loaded
    // with and don't change during the page's lifetime.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  if (state === "missing-params") {
    return (
      <Card>
        <h1 className="font-display text-2xl font-semibold text-ink-900">Invalid verification link</h1>
        <p className="mt-3 text-sm text-ink-700">
          This confirmation link is missing information. Log in and request a new one.
        </p>
        <p className="mt-6 text-center text-sm text-ink-700">
          <Link href="/login" className="font-medium text-beacon-600 hover:underline">
            Back to log in
          </Link>
        </p>
      </Card>
    );
  }

  if (state === "verifying") {
    return (
      <Card>
        <h1 className="font-display text-2xl font-semibold text-ink-900">Confirming your email…</h1>
      </Card>
    );
  }

  if (state === "error") {
    return (
      <Card>
        <h1 className="font-display text-2xl font-semibold text-ink-900">Couldn&apos;t confirm your email</h1>
        <p className="mt-3 text-sm text-ink-700">
          {errorDetail ?? "This confirmation link is invalid or has expired."}
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
      <h1 className="font-display text-2xl font-semibold text-ink-900">Email confirmed</h1>
      <p className="mt-3 text-sm text-ink-700">Your email is confirmed. You can log in now.</p>
      <Link href="/login" className={buttonClasses("primary", "mt-6 w-full")}>
        Go to log in
      </Link>
    </Card>
  );
}

export default function VerifyEmailPage() {
  return (
    <Suspense fallback={null}>
      <VerifyEmailContent />
    </Suspense>
  );
}
