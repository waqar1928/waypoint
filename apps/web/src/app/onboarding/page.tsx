import { redirect } from "next/navigation";
import { getSession } from "@/lib/session";
import { getMyDream } from "@/lib/dream";
import { OnboardingWizard } from "@/components/onboarding/onboarding-wizard";

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
