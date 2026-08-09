import { redirect } from "next/navigation";
import { getMyDream } from "@/lib/dream";
import { getMyExperiments } from "@/lib/experiments";
import { ExperimentsBoard } from "@/components/experiments/experiments-board";

export default async function ExperimentsPage() {
  const dream = await getMyDream();
  if (!dream) {
    redirect("/onboarding");
  }

  const experiments = await getMyExperiments();

  return (
    <div className="mx-auto max-w-3xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Experiment Lab</h1>
      <p className="mt-2 text-ink-700">
        Test assumptions cheaply before you commit. Every experiment is a hypothesis, a way to
        check it, and something you&rsquo;ll actually learn either way.
      </p>
      <div className="mt-8">
        <ExperimentsBoard initialExperiments={experiments} />
      </div>
    </div>
  );
}
