import { getAdminMentors } from "@/lib/admin";
import { AdminMentorsTable } from "@/components/admin/admin-mentors-table";

export default async function AdminMentorsPage() {
  const mentors = await getAdminMentors();

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Mentors</h1>
      <p className="mt-2 text-ink-700">{mentors.length} mentor {mentors.length === 1 ? "profile" : "profiles"}.</p>

      <div className="mt-6">
        <AdminMentorsTable initialMentors={mentors} />
      </div>
    </div>
  );
}
