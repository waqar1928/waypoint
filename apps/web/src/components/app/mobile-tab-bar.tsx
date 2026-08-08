"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { clsx } from "clsx";
import { navItems } from "@/components/app/nav-items";

export function MobileTabBar() {
  const pathname = usePathname();
  const items = navItems.filter((item) => item.available);

  return (
    <nav
      aria-label="Waypoint"
      className="fixed inset-x-0 bottom-0 z-10 flex border-t border-ink-300 bg-paper-raised md:hidden"
    >
      {items.map((item) => {
        const isActive = pathname === item.href || pathname.startsWith(`${item.href}/`);
        return (
          <Link
            key={item.href}
            href={item.href}
            aria-current={isActive ? "page" : undefined}
            className={clsx(
              "flex flex-1 flex-col items-center gap-1 py-3 text-xs font-medium",
              isActive ? "text-beacon-600" : "text-ink-500",
            )}
          >
            <item.icon className="h-5 w-5" aria-hidden="true" />
            {item.label}
          </Link>
        );
      })}
    </nav>
  );
}
