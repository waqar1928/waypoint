import { SiteHeader } from "@/components/marketing/site-header";
import { SiteFooter } from "@/components/marketing/site-footer";

export default function LegalLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen flex-col">
      <SiteHeader />
      <main id="main" className="flex-1 bg-paper">
        <div className="mx-auto max-w-3xl px-4 py-12 sm:px-6 lg:px-10">{children}</div>
      </main>
      <SiteFooter />
    </div>
  );
}
