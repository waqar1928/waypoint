import Link from "next/link";
import { ArrowRight, Compass, FlaskConical, ListChecks, Trophy } from "lucide-react";
import { Card } from "@/components/ui/card";
import { buttonClasses } from "@/components/ui/button";
import { getProfile } from "@/lib/profile";
import { getMyDream } from "@/lib/dream";
import { getRecentJournalEntries } from "@/lib/journal";
import { JournalPanel } from "@/components/app/journal-panel";

const arcStages = ["discover", "define", "validate", "plan", "act", "learn", "grow"] as const;
const arcLabels: Record<(typeof arcStages)[number], string> = {
  discover: "Discover",
  define: "Define",
  validate: "Validate",
  plan: "Plan",
  act: "Act",
  learn: "Learn",
  grow: "Grow",
};

export default async function DashboardPage() {
  const [profile, dream, journalEntries] = await Promise.all([
    getProfile(),
    getMyDream(),
    getRecentJournalEntries(),
  ]);

  const firstName = profile?.displayName?.split(" ")[0] ?? "there";
  const currentStage = dream?.stage ?? "discover";

  return (
    <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">
        Welcome to Waypoint, {firstName}.
      </h1>
      <p className="mt-2 text-ink-700">This is your Dream Dashboard.</p>

      <Card className="mt-8 border-beacon-500/40 bg-beacon-100/40">
        <p className="text-xs font-semibold uppercase tracking-wide text-beacon-600">
          Next best action
        </p>
        {dream ? (
          <>
            <h2 className="mt-2 font-display text-xl font-semibold text-ink-900">
              Keep building on &ldquo;{dream.title}&rdquo;
            </h2>
            <p className="mt-1 text-sm text-ink-700">
              Goals, missions, and actions arrive in a later phase — for now, use your journal to
              capture what you&rsquo;re learning.
            </p>
          </>
        ) : (
          <>
            <h2 className="mt-2 font-display text-xl font-semibold text-ink-900">
              Start Dream Discovery
            </h2>
            <p className="mt-1 text-sm text-ink-700">
              A few honest questions is all it takes to get your first Dream Direction.
            </p>
            <Link href="/onboarding" className={buttonClasses("primary", "mt-4 gap-2")}>
              Start Dream Discovery
              <ArrowRight className="h-4 w-4" aria-hidden="true" />
            </Link>
          </>
        )}
      </Card>

      <div className="mt-6 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {dream ? (
          <Card className="sm:col-span-2 lg:col-span-3">
            <Compass className="h-5 w-5 text-beacon-500" aria-hidden="true" />
            <h3 className="mt-3 font-display text-base font-semibold text-ink-900">{dream.title}</h3>
            <p className="mt-1 text-sm text-ink-700">{dream.statement}</p>
            {dream.purpose ? (
              <p className="mt-2 text-sm text-ink-500">
                <span className="font-medium text-ink-700">Purpose: </span>
                {dream.purpose}
              </p>
            ) : null}
          </Card>
        ) : (
          <EmptyStateCard icon={Compass} title="Your Dream" description="You haven't started Dream Discovery yet." />
        )}
        <EmptyStateCard
          icon={ListChecks}
          title="Current Mission"
          description="Missions appear once Goals & Actions ship in a later phase."
        />
        <EmptyStateCard
          icon={FlaskConical}
          title="Experiments"
          description="No experiments yet — the Experiment Lab opens in a later phase."
        />
        <EmptyStateCard
          icon={Trophy}
          title="Milestones"
          description="Your milestones and achievements will show up here."
        />
      </div>

      <Card className="mt-6">
        <h2 className="font-display text-lg font-semibold text-ink-900">The Waypoint Arc</h2>
        <p className="mt-1 text-sm text-ink-700">Where you are in the journey right now.</p>
        <ol className="mt-5 flex flex-wrap gap-2 text-sm">
          {arcStages.map((stage) => (
            <li
              key={stage}
              className={
                stage === currentStage
                  ? "rounded-full bg-beacon-500 px-4 py-1.5 font-medium text-white"
                  : "rounded-full bg-ink-100 px-4 py-1.5 text-ink-500"
              }
            >
              {arcLabels[stage]}
            </li>
          ))}
        </ol>
      </Card>

      <div className="mt-6">
        <JournalPanel initialEntries={journalEntries} />
      </div>
    </div>
  );
}

function EmptyStateCard({
  icon: Icon,
  title,
  description,
}: {
  icon: typeof Compass;
  title: string;
  description: string;
}) {
  return (
    <Card>
      <Icon className="h-5 w-5 text-ink-500" aria-hidden="true" />
      <h3 className="mt-3 font-display text-base font-semibold text-ink-900">{title}</h3>
      <p className="mt-1 text-sm text-ink-500">{description}</p>
    </Card>
  );
}
