import { getModerationQueue } from "@/lib/admin";
import { ModerationQueue } from "@/components/admin/moderation-queue";

export default async function AdminModerationPage() {
  const reports = await getModerationQueue();

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Moderation queue</h1>
      <p className="mt-2 text-ink-700">{reports.length} open {reports.length === 1 ? "report" : "reports"}.</p>

      <div className="mt-6">
        <ModerationQueue initialReports={reports} />
      </div>
    </div>
  );
}
