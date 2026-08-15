import { redirect } from "next/navigation";
import { getProfile } from "@/lib/profile";
import { getNotificationPreferences } from "@/lib/notification-preferences";
import { getPrivacySettings } from "@/lib/privacy-settings";
import { ProfileForm } from "@/components/app/profile-form";
import { NotificationPreferencesForm } from "@/components/app/notification-preferences-form";
import { PrivacySettingsForm } from "@/components/app/privacy-settings-form";
import { DeleteAccountSection } from "@/components/app/delete-account-section";

export default async function ProfileSettingsPage() {
  // Fetched in parallel since none of the three depend on each other - same pattern the
  // Dashboard page already uses for its own set of independent reads.
  const [profile, notificationPreferences, privacySettings] = await Promise.all([
    getProfile(),
    getNotificationPreferences(),
    getPrivacySettings(),
  ]);
  if (!profile) {
    redirect("/login");
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Settings</h1>
      <p className="mt-2 text-ink-700">
        This is what Drevia Coach will use to personalize your Dream Discovery.
      </p>
      <ProfileForm profile={profile} />
      {/* Every account gets default rows for these on registration (see
          CreateProfileOnUserRegistered.cs), so a null here means the fetch itself failed - not
          that the account has no settings yet - and the section is skipped rather than shown
          broken. */}
      {privacySettings ? <PrivacySettingsForm settings={privacySettings} /> : null}
      {notificationPreferences ? (
        <NotificationPreferencesForm preferences={notificationPreferences} />
      ) : null}
      <DeleteAccountSection />
    </div>
  );
}
