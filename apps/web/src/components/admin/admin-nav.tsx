"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { clsx } from "clsx";
import {
  ShieldCheck,
  LayoutDashboard,
  Users,
  Compass,
  Flag,
  HeartHandshake,
  Sparkles,
  ScrollText,
  ArrowLeft,
} from "lucide-react";

const adminNavItems = [
  { label: "Overview", href: "/admin", icon: LayoutDashboard },
  { label: "Users", href: "/admin/users", icon: Users },
  { label: "Dreams", href: "/admin/dreams", icon: Compass },
  { label: "Moderation", href: "/admin/moderation", icon: Flag },
  { label: "Mentors", href: "/admin/mentors", icon: HeartHandshake },
  { label: "AI Usage", href: "/admin/ai-usage", icon: Sparkles },
  { label: "System Health", href: "/admin/system-health", icon: ScrollText },
];

export function AdminNav() {
  const pathname = usePathname();

  return (
    <>
      <nav
        aria-label="Admin"
        className="hidden w-64 shrink-0 flex-col bg-chart-900 px-4 py-6 text-paper md:flex"
      >
        <Link href="/admin" className="mb-2 flex items-center gap-2 px-2 font-display text-lg font-semibold">
          <ShieldCheck className="h-5 w-5 text-beacon-500" aria-hidden="true" />
          Admin
        </Link>
        <Link
          href="/app/dashboard"
          className="mb-6 flex items-center gap-2 px-2 text-xs text-ink-100/70 hover:text-paper"
        >
          <ArrowLeft className="h-3.5 w-3.5" aria-hidden="true" />
          Back to app
        </Link>
        <ul className="flex flex-1 flex-col gap-1">
          {adminNavItems.map((item) => {
            const isActive = pathname === item.href || pathname.startsWith(`${item.href}/`);
            return (
              <li key={item.href}>
                <Link
                  href={item.href}
                  aria-current={isActive ? "page" : undefined}
                  className={clsx(
                    "flex items-center gap-3 rounded-[10px] px-3 py-2.5 text-sm font-medium transition-colors focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2",
                    isActive ? "bg-beacon-500 text-white" : "text-ink-100 hover:bg-white/10",
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

      <nav
        aria-label="Admin"
        className="fixed inset-x-0 top-16 z-10 flex overflow-x-auto border-b border-ink-300 bg-paper-raised md:hidden"
      >
        {adminNavItems.map((item) => {
          const isActive = pathname === item.href || pathname.startsWith(`${item.href}/`);
          return (
            <Link
              key={item.href}
              href={item.href}
              aria-current={isActive ? "page" : undefined}
              className={clsx(
                "flex min-w-[74px] shrink-0 flex-col items-center gap-1 px-1 py-3 text-center text-xs font-medium",
                isActive ? "text-beacon-600" : "text-ink-500",
              )}
            >
              <item.icon className="h-5 w-5" aria-hidden="true" />
              {item.label}
            </Link>
          );
        })}
      </nav>
    </>
  );
}
