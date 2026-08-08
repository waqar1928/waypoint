import { redirect } from "next/navigation";
import { getProfile } from "@/lib/profile";
import { ProfileForm } from "@/components/app/profile-form";

export default async function ProfileSettingsPage() {
  const profile = await getProfile();
  if (!profile) {
    redirect("/login");
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Profile</h1>
      <p className="mt-2 text-ink-700">
        This is what Waypoint Coach will use to personalize your Dream Discovery.
      </p>
      <ProfileForm profile={profile} />
    </div>
  );
}
