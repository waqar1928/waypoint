import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";

export type ActionPriority = "low" | "medium" | "high";
export type ActionDifficulty = "easy" | "medium" | "hard";
export type ActionImpact = "low" | "medium" | "high";
export type ActionStatus = "notStarted" | "inProgress" | "completed" | "blocked" | "cancelled";

export interface WaypointAction {
  id: string;
  title: string;
  description: string | null;
  priority: ActionPriority;
  difficulty: ActionDifficulty;
  estimatedMinutes: number | null;
  expectedImpact: ActionImpact;
  dueDate: string | null;
  status: ActionStatus;
  isNextBestAction: boolean;
  goalId: string | null;
  missionId: string | null;
  /** A one-line, plain-language reason this is the recommended next move - only ever set on the
   * result of getNextBestAction(), null everywhere else. See NextBestActionSelector on the
   * backend for how it's computed. */
  rationale: string | null;
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

export async function getMyActions(): Promise<WaypointAction[]> {
  return (await serverFetch<WaypointAction[]>("/api/v1/actions")) ?? [];
}

export async function getNextBestAction(): Promise<WaypointAction | null> {
  return serverFetch<WaypointAction>("/api/v1/actions/next-best");
}
