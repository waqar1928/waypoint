import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";

export type GoalHorizon = "fiveYear" | "threeYear" | "oneYear";

export interface Goal {
  id: string;
  horizon: GoalHorizon;
  statement: string;
  targetDate: string | null;
}

export interface Mission {
  id: string;
  goalId: string;
  title: string;
  targetDate: string | null;
}

export interface Plan {
  goals: Goal[];
  missions: Mission[];
}

export interface PlanDraft {
  fiveYearVision: string;
  threeYearDirection: string;
  oneYearGoal: string;
  ninetyDayMission: string;
}

export interface Milestone {
  id: string;
  title: string;
  achievedAt: string | null;
  isCustom: boolean;
}

async function serverFetch<T>(path: string): Promise<T | null> {
  const cookieStore = await cookies();
  const cookieHeader = cookieStore.toString();
  if (!cookieHeader) return null;

  try {
    const response = await fetch(`${API_BASE_URL}${path}`, {
      headers: { cookie: cookieHeader },
      cache: "no-store",
    });
    if (!response.ok) return null;
    return (await response.json()) as T;
  } catch {
    return null;
  }
}

export async function getMyPlan(): Promise<Plan | null> {
  return serverFetch<Plan>("/api/v1/plan/me");
}

export async function getMyMilestones(): Promise<Milestone[]> {
  return (await serverFetch<Milestone[]>("/api/v1/milestones")) ?? [];
}
