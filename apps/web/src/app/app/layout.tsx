import { redirect } from "next/navigation";
import { getSession } from "@/lib/session";
import { NavRail } from "@/components/app/nav-rail";
import { MobileTabBar } from "@/components/app/mobile-tab-bar";
import { TopBar } from "@/components/app/top-bar";

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
