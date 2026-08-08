import { redirect } from "next/navigation";
import { getMyDream } from "@/lib/dream";
import { DreamEditForm } from "@/components/app/dream-edit-form";

export default async function DreamPage() {
  const dream = await getMyDream();
  if (!dream) {
    redirect("/onboarding");
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Your Dream</h1>
      <p className="mt-2 text-ink-700">Edit anything here as your thinking sharpens.</p>
      <div className="mt-8">
        <DreamEditForm dream={dream} />
      </div>
    </div>
  );
}
