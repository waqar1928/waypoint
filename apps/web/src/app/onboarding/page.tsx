import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { getSession } from "@/lib/session";
import { getMyDream } from "@/lib/dream";
import { OnboardingWizard } from "@/components/onboarding/onboarding-wizard";

// See apps/web/src/app/app/layout.tsx's identical export for why this exists — onboarding is
// private/auth-gated same as /app and /admin, just without its own layout.tsx to attach this to.
export const metadata: Metadata = {
  robots: { index: false, follow: false },
};

export default async function OnboardingPage() {
  const session = await getSession();
  if (!session) {
    redirect("/login");
  }

  const dream = await getMyDream();
  if (dream) {
    redirect("/app/dashboard");
  }

  return <OnboardingWizard />;
}
