import Link from "next/link";
import { ArrowRight, Compass, FlaskConical, ListChecks, Trophy } from "lucide-react";
import { Card } from "@/components/ui/card";
import { buttonClasses } from "@/components/ui/button";
import { getProfile } from "@/lib/profile";

export default async function DashboardPage() {
  const profile = await getProfile();
  const firstName = profile?.displayName?.split(" ")[0] ?? "there";
  const profileComplete = Boolean(profile?.bio && profile?.timeZone);

  return (
    <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">
        Welcome to Waypoint, {firstName}.
      </h1>
      <p className="mt-2 text-ink-700">
        This is your Dream Dashboard. Discovery, planning, and the Waypoint Coach arrive in a
        later phase — for now, here&rsquo;s your foundation.
      </p>

      <Card className="mt-8 border-beacon-500/40 bg-beacon-100/40">
        <p className="text-xs font-semibold uppercase tracking-wide text-beacon-600">
          Next best action
        </p>
        {profileComplete ? (
          <>
            <h2 className="mt-2 font-display text-xl font-semibold text-ink-900">
              You&rsquo;re all set for now
            </h2>
            <p className="mt-1 text-sm text-ink-700">
              Dream Discovery isn&rsquo;t open yet — we&rsquo;ll let you know the moment it is.
            </p>
          </>
        ) : (
          <>
            <h2 className="mt-2 font-display text-xl font-semibold text-ink-900">
              Finish setting up your profile
            </h2>
            <p className="mt-1 text-sm text-ink-700">
              A complete profile helps Waypoint Coach personalize your Dream Discovery when it
              opens.
            </p>
            <Link
              href="/app/settings/profile"
              className={buttonClasses("primary", "mt-4 gap-2")}
            >
              Complete your profile
              <ArrowRight className="h-4 w-4" aria-hidden="true" />
            </Link>
          </>
        )}
      </Card>

      <div className="mt-6 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        <EmptyStateCard
          icon={Compass}
          title="Your Dream"
          description="You haven't started Dream Discovery yet."
        />
        <EmptyStateCard
          icon={ListChecks}
          title="Current Mission"
          description="Missions appear once your dream is defined."
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
          {["Discover", "Define", "Validate", "Plan", "Act", "Learn", "Grow"].map(
            (stage, index) => (
              <li
                key={stage}
                className={
                  index === 0
                    ? "rounded-full bg-beacon-500 px-4 py-1.5 font-medium text-white"
                    : "rounded-full bg-ink-100 px-4 py-1.5 text-ink-500"
                }
              >
                {stage}
              </li>
            ),
          )}
        </ol>
      </Card>
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
