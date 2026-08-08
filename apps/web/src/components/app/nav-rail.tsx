"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Compass } from "lucide-react";
import { clsx } from "clsx";
import { navItems } from "@/components/app/nav-items";

export function NavRail() {
  const pathname = usePathname();

  return (
    <nav
      aria-label="Waypoint"
      className="hidden w-64 shrink-0 flex-col bg-chart-900 px-4 py-6 text-paper md:flex"
    >
      <Link href="/" className="mb-8 flex items-center gap-2 px-2 font-display text-lg font-semibold">
        <Compass className="h-5 w-5 text-beacon-500" aria-hidden="true" />
        Waypoint
      </Link>
      <ul className="flex flex-1 flex-col gap-1">
        {navItems.map((item) => {
          const isActive = pathname === item.href || pathname.startsWith(`${item.href}/`);
          if (!item.available) {
            return (
              <li key={item.href}>
                <span
                  aria-disabled="true"
                  className="flex items-center justify-between rounded-[10px] px-3 py-2.5 text-sm text-ink-300/50"
                >
                  <span className="flex items-center gap-3">
                    <item.icon className="h-4 w-4" aria-hidden="true" />
                    {item.label}
                  </span>
                  <span className="rounded-full bg-white/10 px-2 py-0.5 text-[11px] font-medium">
                    Soon
                  </span>
                </span>
              </li>
            );
          }
          return (
            <li key={item.href}>
              <Link
                href={item.href}
                aria-current={isActive ? "page" : undefined}
                className={clsx(
                  "flex items-center gap-3 rounded-[10px] px-3 py-2.5 text-sm font-medium transition-colors focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2",
                  isActive
                    ? "bg-beacon-500 text-white"
                    : "text-ink-100 hover:bg-white/10",
                )}
              >
                <item.icon className="h-4 w-4" aria-hidden="true" />
                {item.label}
              </Link>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}
