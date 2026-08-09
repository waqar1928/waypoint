import Link from "next/link";
import { ArrowRight, Building2, Compass, FlaskConical, ListChecks, MessageCircleQuestion, Users, HeartHandshake } from "lucide-react";
import { Card } from "@/components/ui/card";
import { buttonClasses } from "@/components/ui/button";
import { getProfile } from "@/lib/profile";
import { getMyDream } from "@/lib/dream";
import { getRecentJournalEntries } from "@/lib/journal";
import { getMyPlan, getMyMilestones } from "@/lib/plan";
import { getNextBestAction } from "@/lib/actions";
import { getMyExperiments } from "@/lib/experiments";
import { getMyBusinessValidations } from "@/lib/business";
import { getMyConversations } from "@/lib/coach";
import { getMyPosts } from "@/lib/community";
import { getMyHelpRequests } from "@/lib/mentorship";
import { JournalPanel } from "@/components/app/journal-panel";
import { MilestonesPanel } from "@/components/app/milestones-panel";

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
  const [
    profile, dream, journalEntries, plan, milestones, nextBestAction, experiments,
    businessValidations, conversations, myPosts, myHelpRequests,
  ] = await Promise.all([
    getProfile(),
    getMyDream(),
    getRecentJournalEntries(),
    getMyPlan(),
    getMyMilestones(),
    getNextBestAction(),
    getMyExperiments(),
    getMyBusinessValidations(),
    getMyConversations(),
    getMyPosts(),
    getMyHelpRequests(),
  ]);
  const latestViability = businessValidations[0] ?? null;

  const firstName = profile?.displayName?.split(" ")[0] ?? "there";
  const currentStage = dream?.stage ?? "discover";
  const mission = plan?.missions[0];

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
        {nextBestAction ? (
          <>
            <h2 className="mt-2 font-display text-xl font-semibold text-ink-900">
              {nextBestAction.title}
            </h2>
            {nextBestAction.description ? (
              <p className="mt-1 text-sm text-ink-700">{nextBestAction.description}</p>
            ) : null}
            <Link href="/app/actions" className={buttonClasses("primary", "mt-4 gap-2")}>
              Go to Actions
              <ArrowRight className="h-4 w-4" aria-hidden="true" />
            </Link>
          </>
        ) : dream && !plan ? (
          <>
            <h2 className="mt-2 font-display text-xl font-semibold text-ink-900">Draft your plan</h2>
            <p className="mt-1 text-sm text-ink-700">
              Turn &ldquo;{dream.title}&rdquo; into a 90-day mission you can actually start on.
            </p>
            <Link href="/app/plan" className={buttonClasses("primary", "mt-4 gap-2")}>
              Go to Plan
              <ArrowRight className="h-4 w-4" aria-hidden="true" />
            </Link>
          </>
        ) : dream && plan ? (
          <>
            <h2 className="mt-2 font-display text-xl font-semibold text-ink-900">Choose your next best action</h2>
            <p className="mt-1 text-sm text-ink-700">
              You have a plan — add an action and mark it as next so you always know what to do today.
            </p>
            <Link href="/app/actions" className={buttonClasses("primary", "mt-4 gap-2")}>
              Go to Actions
              <ArrowRight className="h-4 w-4" aria-hidden="true" />
            </Link>
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

        {mission ? (
          <Card>
            <ListChecks className="h-5 w-5 text-beacon-500" aria-hidden="true" />
            <h3 className="mt-3 font-display text-base font-semibold text-ink-900">Current Mission</h3>
            <p className="mt-1 text-sm text-ink-700">{mission.title}</p>
          </Card>
        ) : (
          <EmptyStateCard
            icon={ListChecks}
            title="Current Mission"
            description="Missions appear once you draft your plan."
          />
        )}

        {experiments.length > 0 ? (
          <Card>
            <FlaskConical className="h-5 w-5 text-beacon-500" aria-hidden="true" />
            <h3 className="mt-3 font-display text-base font-semibold text-ink-900">Experiments</h3>
            <p className="mt-1 text-sm text-ink-700">
              {experiments.length} {experiments.length === 1 ? "experiment" : "experiments"} ·{" "}
              {experiments.filter((e) => e.status === "running").length} running
            </p>
            <Link href="/app/experiments" className="mt-3 inline-block text-sm font-medium text-beacon-600 hover:underline">
              Go to Experiment Lab
            </Link>
          </Card>
        ) : (
          <Card>
            <FlaskConical className="h-5 w-5 text-ink-500" aria-hidden="true" />
            <h3 className="mt-3 font-display text-base font-semibold text-ink-900">Experiments</h3>
            <p className="mt-1 text-sm text-ink-500">
              Test the smallest possible next step before you commit to it.
            </p>
            <Link href="/app/experiments" className="mt-3 inline-block text-sm font-medium text-beacon-600 hover:underline">
              Start an experiment
            </Link>
          </Card>
        )}

        {dream?.isBusinessShaped ? (
          <Card>
            <Building2 className="h-5 w-5 text-beacon-500" aria-hidden="true" />
            <h3 className="mt-3 font-display text-base font-semibold text-ink-900">Business Builder</h3>
            <p className="mt-1 text-sm text-ink-700">
              {latestViability
                ? `Latest viability estimate: ${latestViability.viabilityEstimate ?? "—"}/100`
                : "Fill in your business profile to get a viability estimate."}
            </p>
            <Link href="/app/business" className="mt-3 inline-block text-sm font-medium text-beacon-600 hover:underline">
              Go to Business Builder
            </Link>
          </Card>
        ) : null}

        <Card>
          <MessageCircleQuestion className="h-5 w-5 text-beacon-500" aria-hidden="true" />
          <h3 className="mt-3 font-display text-base font-semibold text-ink-900">Waypoint Coach</h3>
          <p className="mt-1 text-sm text-ink-700">
            {conversations.length > 0
              ? `${conversations.length} ${conversations.length === 1 ? "conversation" : "conversations"} so far.`
              : "A collaborative guide, not a verdict — talk through your dream anytime."}
          </p>
          <Link href="/app/coach" className="mt-3 inline-block text-sm font-medium text-beacon-600 hover:underline">
            Talk to Coach
          </Link>
        </Card>

        <Card>
          <Users className="h-5 w-5 text-beacon-500" aria-hidden="true" />
          <h3 className="mt-3 font-display text-base font-semibold text-ink-900">Community</h3>
          <p className="mt-1 text-sm text-ink-700">
            {myPosts.length > 0
              ? `${myPosts.length} ${myPosts.length === 1 ? "post" : "posts"} shared so far.`
              : "An opt-in space to share progress with other Waypoint members."}
          </p>
          <Link href="/app/community" className="mt-3 inline-block text-sm font-medium text-beacon-600 hover:underline">
            Go to Community
          </Link>
        </Card>

        <Card>
          <HeartHandshake className="h-5 w-5 text-beacon-500" aria-hidden="true" />
          <h3 className="mt-3 font-display text-base font-semibold text-ink-900">Mentorship</h3>
          <p className="mt-1 text-sm text-ink-700">
            {myHelpRequests.length > 0
              ? `${myHelpRequests.filter((r) => r.status !== "closed").length} open help ${myHelpRequests.filter((r) => r.status !== "closed").length === 1 ? "request" : "requests"}.`
              : "Ask for help, or opt in to give it."}
          </p>
          <Link href="/app/mentorship" className="mt-3 inline-block text-sm font-medium text-beacon-600 hover:underline">
            Go to Mentorship
          </Link>
        </Card>
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

      <div className="mt-6 grid gap-6 lg:grid-cols-2">
        <MilestonesPanel initialMilestones={milestones} />
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
