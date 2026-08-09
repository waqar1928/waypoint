import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";

export type ExperimentStatus = "planned" | "running" | "completed";
export type ExperimentOutcome = "validated" | "partiallyValidated" | "invalidated";

export interface ExperimentResult {
  id: string;
  outcome: ExperimentOutcome;
  evidence: string | null;
  learning: string;
  createdAt: string;
}

export interface Experiment {
  id: string;
  ideaDescription: string;
  hypothesis: string;
  successCriteria: string;
  status: ExperimentStatus;
  results: ExperimentResult[];
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

export async function getMyExperiments(): Promise<Experiment[]> {
  return (await serverFetch<Experiment[]>("/api/v1/experiments")) ?? [];
}
