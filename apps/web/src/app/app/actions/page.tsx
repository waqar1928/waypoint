import { redirect } from "next/navigation";
import { getMyDream } from "@/lib/dream";
import { getMyActions } from "@/lib/actions";
import { ActionsBoard } from "@/components/actions/actions-board";

export default async function ActionsPage() {
  const dream = await getMyDream();
  if (!dream) {
    redirect("/onboarding");
  }

  const actions = await getMyActions();

  return (
    <div className="mx-auto max-w-3xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Actions</h1>
      <p className="mt-2 text-ink-700">What should you do today? One action can be your next best.</p>
      <div className="mt-8">
        <ActionsBoard initialActions={actions} />
      </div>
    </div>
  );
}
