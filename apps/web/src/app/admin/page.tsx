import Link from "next/link";
import { Users, Compass, Flag, HeartHandshake, Sparkles, ScrollText, ArrowRight } from "lucide-react";
import { Card } from "@/components/ui/card";
import {
  getAdminUsers,
  getAdminDreams,
  getModerationQueue,
  getAdminMentors,
  getAiUsageSummary,
} from "@/lib/admin";

export default async function AdminOverviewPage() {
  const [users, dreams, reports, mentors, aiUsage] = await Promise.all([
    getAdminUsers(),
    getAdminDreams(),
    getModerationQueue(),
    getAdminMentors(),
    getAiUsageSummary(),
  ]);

  const pendingMentors = mentors.filter((m) => m.verificationStatus !== "verified").length;

  const tiles = [
    { label: "Users", value: users.length, href: "/admin/users", icon: Users },
    { label: "Dreams", value: dreams.length, href: "/admin/dreams", icon: Compass },
    { label: "Open reports", value: reports.length, href: "/admin/moderation", icon: Flag },
    { label: "Unverified mentors", value: pendingMentors, href: "/admin/mentors", icon: HeartHandshake },
    { label: "AI conversations", value: aiUsage?.totalConversations ?? 0, href: "/admin/ai-usage", icon: Sparkles },
    { label: "Audit log", value: "View", href: "/admin/system-health", icon: ScrollText },
  ];

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Admin overview</h1>
      <p className="mt-2 text-ink-700">Staff-only tools for users, content moderation, and platform health.</p>

      <div className="mt-8 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {tiles.map((tile) => (
          <Link key={tile.href} href={tile.href}>
            <Card className="transition-colors hover:border-beacon-500">
              <div className="flex items-center justify-between">
                <tile.icon className="h-5 w-5 text-beacon-500" aria-hidden="true" />
                <ArrowRight className="h-4 w-4 text-ink-500" aria-hidden="true" />
              </div>
              <p className="mt-3 font-display text-2xl font-semibold text-ink-900">{tile.value}</p>
              <p className="mt-1 text-sm text-ink-700">{tile.label}</p>
            </Card>
          </Link>
        ))}
      </div>
    </div>
  );
}
