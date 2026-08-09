import { Card } from "@/components/ui/card";
import { getAdminDreams } from "@/lib/admin";

const stageLabels: Record<string, string> = {
  discover: "Discover",
  define: "Define",
  validate: "Validate",
  plan: "Plan",
  act: "Act",
  learn: "Learn",
  grow: "Grow",
};

export default async function AdminDreamsPage() {
  const dreams = await getAdminDreams();

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Dreams</h1>
      <p className="mt-2 text-ink-700">{dreams.length} dreams across all users.</p>

      <div className="mt-6">
        {dreams.length === 0 ? (
          <Card>
            <p className="text-sm text-ink-500">No dreams yet.</p>
          </Card>
        ) : (
          <Card className="p-0">
            <ul className="divide-y divide-ink-300">
              {dreams.map((dream) => (
                <li key={dream.id} className="p-4">
                  <div className="flex items-center justify-between gap-3">
                    <p className="truncate text-sm font-medium text-ink-900">{dream.title}</p>
                    <span className="shrink-0 rounded-full bg-ink-100 px-2 py-0.5 text-[11px] font-medium text-ink-700">
                      {stageLabels[dream.stage] ?? dream.stage}
                    </span>
                  </div>
                  <p className="mt-1 text-xs text-ink-500">
                    {dream.ownerDisplayName ?? "Unknown owner"}
                    {dream.isBusinessShaped ? " · Business-shaped" : ""}
                  </p>
                </li>
              ))}
            </ul>
          </Card>
        )}
      </div>
    </div>
  );
}
