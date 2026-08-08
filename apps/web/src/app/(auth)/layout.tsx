import Link from "next/link";
import { Compass } from "lucide-react";

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen flex-1 flex-col items-center justify-center bg-paper px-4 py-16">
      <Link
        href="/"
        className="mb-8 flex items-center gap-2 font-display text-lg font-semibold text-ink-900"
      >
        <Compass className="h-5 w-5 text-beacon-500" aria-hidden="true" />
        Waypoint
      </Link>
      <div className="w-full max-w-sm">{children}</div>
    </div>
  );
}
