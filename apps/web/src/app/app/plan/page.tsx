import { redirect } from "next/navigation";
import { getMyDream } from "@/lib/dream";
import { getMyPlan } from "@/lib/plan";
import { PlanView } from "@/components/plan/plan-view";

export default async function PlanPage() {
  const dream = await getMyDream();
  if (!dream) {
    redirect("/onboarding");
  }

  const plan = await getMyPlan();

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Your Plan</h1>
      <p className="mt-2 text-ink-700">
        From &ldquo;{dream.title}&rdquo; to a 90-day mission you can actually start on.
      </p>
      <div className="mt-8">
        <PlanView initialPlan={plan} />
      </div>
    </div>
  );
}
