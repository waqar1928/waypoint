import { redirect } from "next/navigation";
import { getSession } from "@/lib/session";
import { AdminNav } from "@/components/admin/admin-nav";
import { TopBar } from "@/components/app/top-bar";

export default async function AdminLayout({ children }: { children: React.ReactNode }) {
  const session = await getSession();
  if (!session) {
    redirect("/login");
  }
  if (!session.isAdmin) {
    redirect("/app/dashboard");
  }

  return (
    <div className="flex min-h-screen bg-paper">
      <AdminNav />
      <div className="flex flex-1 flex-col">
        <TopBar email={session.email} />
        <main id="main" className="flex-1 pt-14 md:pt-0">
          {children}
        </main>
      </div>
    </div>
  );
}
