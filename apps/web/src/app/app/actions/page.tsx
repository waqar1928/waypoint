import { redirect } from "next/navigation";
import { getMyDream } from "@/lib/dream";
import { getMyActions, getNextBestAction } from "@/lib/actions";
import { nextMoveRationaleText } from "@/lib/next-move";
import { ActionsBoard } from "@/components/actions/actions-board";

export default async function ActionsPage() {
  const dream = await getMyDream();
  if (!dream) {
    redirect("/onboarding");
  }

  // Same /next-best call Dashboard and Dream Overview already use - see NextBestActionSelector on
  // the backend. getMyActions() alone can't tell us which row is "the" next best action: its
  // isNextBestAction field only reflects a manual pin, not the computed fallback, which is exactly
  // the gap this list used to have (a computed recommendation with no pin showed no indicator at
  // all here, while Dashboard/Dream Overview showed it correctly).
  const [actions, nextBestAction] = await Promise.all([getMyActions(), getNextBestAction()]);

  return (
    <div className="mx-auto max-w-3xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Actions</h1>
      <p className="mt-2 text-ink-700">What should you do today? One action can be your next best.</p>
      <div className="mt-8">
        <ActionsBoard
          initialActions={actions}
          initialNextBestActionId={nextBestAction?.id ?? null}
          initialNextBestRationale={nextMoveRationaleText(nextBestAction)}
        />
      </div>
    </div>
  );
}
