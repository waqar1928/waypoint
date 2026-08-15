import type { LucideIcon } from "lucide-react";
import {
  LayoutDashboard,
  Compass,
  Map,
  ListChecks,
  FlaskConical,
  Building2,
  MessageCircleQuestion,
  Lightbulb,
  BookOpen,
  History,
  Settings,
  Users,
  HeartHandshake,
} from "lucide-react";

export interface NavItem {
  label: string;
  href: string;
  icon: LucideIcon;
  /** Whether this module has shipped yet — unavailable items render disabled with a "Soon" badge instead of a dead link. */
  available: boolean;
}

export const navItems: NavItem[] = [
  { label: "Dashboard", href: "/app/dashboard", icon: LayoutDashboard, available: true },
  { label: "Dream", href: "/app/dream", icon: Compass, available: true },
  { label: "Plan", href: "/app/plan", icon: Map, available: true },
  { label: "Actions", href: "/app/actions", icon: ListChecks, available: true },
  { label: "Experiment Lab", href: "/app/experiments", icon: FlaskConical, available: true },
  { label: "Business Builder", href: "/app/business", icon: Building2, available: true },
  { label: "Drevia Coach", href: "/app/coach", icon: MessageCircleQuestion, available: true },
  { label: "Community", href: "/app/community", icon: Users, available: true },
  { label: "Mentorship", href: "/app/mentorship", icon: HeartHandshake, available: true },
  { label: "Idea Studio", href: "/app/ideas", icon: Lightbulb, available: false },
  // Journal has no dedicated page - it's the JournalPanel rendered directly on the Dashboard
  // (see dashboard/page.tsx). This used to be marked available: false with a dead /app/journal
  // link, which told users a real, working feature didn't exist. Routes to the Dashboard's
  // #journal anchor instead of a standalone page - see docs/DREVIA_PRODUCT_SPECIFICATION.md.
  { label: "Journal", href: "/app/dashboard#journal", icon: BookOpen, available: true },
  { label: "Timeline", href: "/app/timeline", icon: History, available: false },
  { label: "Settings", href: "/app/settings/profile", icon: Settings, available: true },
];
