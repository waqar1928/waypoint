import Link from "next/link";
import { Compass } from "lucide-react";
import { buttonClasses } from "@/components/ui/button";

export function SiteHeader() {
  return (
    <header className="border-b border-ink-300 bg-paper/90 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-6xl items-center justify-between px-4 sm:px-6 lg:px-10">
        <Link href="/" className="flex items-center gap-2 font-display text-lg font-semibold">
          <Compass className="h-5 w-5 text-beacon-500" aria-hidden="true" />
          Waypoint
        </Link>
        <nav aria-label="Primary" className="hidden items-center gap-8 text-sm font-medium text-ink-700 md:flex">
          <Link href="/#how-it-works" className="hover:text-ink-900">How it works</Link>
          <Link href="/#features" className="hover:text-ink-900">Features</Link>
          <Link href="/#faq" className="hover:text-ink-900">FAQ</Link>
        </nav>
        <div className="flex items-center gap-3">
          <Link href="/login" className="hidden text-sm font-medium text-ink-700 hover:text-ink-900 sm:block">
            Log in
          </Link>
          <Link href="/register" className={buttonClasses("primary", "px-4")}>
            Find My Dream
          </Link>
        </div>
      </div>
    </header>
  );
}
