import type { Metadata } from "next";
import Link from "next/link";
import { Compass, ArrowRight } from "lucide-react";
import { SiteHeader } from "@/components/marketing/site-header";
import { SiteFooter } from "@/components/marketing/site-footer";
import { buttonClasses } from "@/components/ui/button";

// Next.js already returns a real 404 HTTP status for this page automatically - this just keeps
// search engines from indexing it, same as every other non-content page in the app.
export const metadata: Metadata = {
  title: "Page not found | Drevia",
  robots: { index: false, follow: false },
};

export default function NotFound() {
  return (
    <>
      <SiteHeader />
      <main id="main" className="flex flex-1 flex-col items-center justify-center px-4 py-24 text-center">
        <Compass className="h-10 w-10 text-beacon-500" aria-hidden="true" />
        <h1 className="mt-6 font-display text-3xl font-semibold text-ink-900 sm:text-4xl">
          Looks like this path doesn&rsquo;t lead anywhere.
        </h1>
        <p className="mt-3 max-w-md text-ink-700">Let&rsquo;s get you back to Drevia.</p>
        <div className="mt-8 flex flex-col items-center gap-3 sm:flex-row">
          <Link href="/" className={buttonClasses("primary", "gap-2")}>
            Back to homepage
            <ArrowRight className="h-4 w-4" aria-hidden="true" />
          </Link>
          <Link href="/app/dashboard" className={buttonClasses("secondary")}>
            Go to your dashboard
          </Link>
        </div>
      </main>
      <SiteFooter />
    </>
  );
}
