import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";

export interface BusinessIdea {
  id: string;
  problem: string | null;
  customer: string | null;
  valueProposition: string | null;
  solution: string | null;
  businessModel: string | null;
  market: string | null;
  competitors: string | null;
  pricing: string | null;
  marketing: string | null;
  sales: string | null;
  operations: string | null;
  technology: string | null;
  financialAssumptions: string | null;
  risks: string | null;
}

export interface BusinessValidation {
  id: string;
  viabilityEstimate: number | null;
  strongAssumptions: string[];
  weakAssumptions: string[];
  unknowns: string[];
  recommendedExperiments: string[];
  generatedByAi: boolean;
  createdAt: string;
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

export async function getMyBusinessIdea(): Promise<BusinessIdea | null> {
  return serverFetch<BusinessIdea>("/api/v1/business-idea");
}

export async function getMyBusinessValidations(): Promise<BusinessValidation[]> {
  return (await serverFetch<BusinessValidation[]>("/api/v1/business-idea/validations")) ?? [];
}
