import { redirect } from "next/navigation";
import { getMyDream } from "@/lib/dream";
import { getMyPlan } from "@/lib/plan";
import { getNextBestAction } from "@/lib/actions";
import { getMyExperiments } from "@/lib/experiments";
import { getRecentLearnings } from "@/lib/journal";
import { DreamOverview } from "@/components/app/dream-overview";

export default async function DreamPage() {
  const dream = await getMyDream();
  if (!dream) {
    redirect("/onboarding");
  }

  const [plan, nextBestAction, experiments, learnings] = await Promise.all([
    getMyPlan(),
    getNextBestAction(),
    getMyExperiments(),
    getRecentLearnings(),
  ]);

  const currentMission = plan?.missions[0] ?? null;
  const activeExperiment =
    experiments.find((e) => e.status === "running") ?? experiments.find((e) => e.status === "planned") ?? null;

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <DreamOverview
        dream={dream}
        currentMission={currentMission}
        nextBestAction={nextBestAction}
        activeExperiment={activeExperiment}
        recentLearnings={learnings.slice(0, 3)}
      />
    </div>
  );
}
