import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { getSession } from "@/lib/session";
import { NavRail } from "@/components/app/nav-rail";
import { MobileTabBar } from "@/components/app/mobile-tab-bar";
import { TopBar } from "@/components/app/top-bar";

// Defense-in-depth alongside robots.ts's Disallow: /app/ — a well-behaved crawler never requests
// these paths at all, but this covers the case where a private URL gets linked from somewhere
// else regardless (see docs/PRODUCTION_READINESS_AUDIT.md's SEO section). Auth already blocks
// unauthenticated access, so this is belt-and-suspenders, not the only protection.
export const metadata: Metadata = {
  robots: { index: false, follow: false },
};

export default async function AppLayout({ children }: { children: React.ReactNode }) {
  const session = await getSession();
  if (!session) {
    redirect("/login");
  }

  return (
    <div className="flex min-h-screen bg-paper">
      <NavRail />
      <div className="flex flex-1 flex-col">
        <TopBar email={session.email} />
        <main id="main" className="flex-1 pb-20 md:pb-0">
          {children}
        </main>
      </div>
      <MobileTabBar />
    </div>
  );
}
