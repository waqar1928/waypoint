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
  { label: "Experiment Lab", href: "/app/experiments", icon: FlaskConical, available: false },
  { label: "Business Builder", href: "/app/business", icon: Building2, available: false },
  { label: "Waypoint Coach", href: "/app/coach", icon: MessageCircleQuestion, available: false },
  { label: "Idea Studio", href: "/app/ideas", icon: Lightbulb, available: false },
  { label: "Journal", href: "/app/journal", icon: BookOpen, available: false },
  { label: "Timeline", href: "/app/timeline", icon: History, available: false },
  { label: "Settings", href: "/app/settings/profile", icon: Settings, available: true },
];
