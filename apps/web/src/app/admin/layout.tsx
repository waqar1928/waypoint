import type { Metadata } from "next";
import { redirect } from "next/navigation";
import { getSession } from "@/lib/session";
import { AdminNav } from "@/components/admin/admin-nav";
import { TopBar } from "@/components/app/top-bar";

// See apps/web/src/app/app/layout.tsx's identical export for why this exists.
export const metadata: Metadata = {
  robots: { index: false, follow: false },
};

export default async function AdminLayout({ children }: { children: React.ReactNode }) {
  const session = await getSession();
  if (!session) {
    redirect("/login");
  }
  // Moderator-only accounts are allowed into the admin shell (their nav is filtered to only the
  // sections the backend's "Moderator" policy actually authorizes — see AdminNav and
  // docs/PRODUCTION_READINESS_AUDIT.md's Authorization section) — everyone else is turned away.
  if (!session.isAdmin && !session.isModerator) {
    redirect("/app/dashboard");
  }

  return (
    <div className="flex min-h-screen bg-paper">
      <AdminNav isAdmin={session.isAdmin} />
      <div className="flex flex-1 flex-col">
        <TopBar email={session.email} />
        <main id="main" className="flex-1 pt-14 md:pt-0">
          {children}
        </main>
      </div>
    </div>
  );
}
