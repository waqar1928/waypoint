import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";

export type DreamStage = "discover" | "define" | "validate" | "plan" | "act" | "learn" | "grow";

export interface Dream {
  id: string;
  title: string;
  stage: DreamStage;
  isBusinessShaped: boolean;
  statement: string;
  purpose: string | null;
  whoItHelps: string | null;
  problem: string | null;
  outcome: string | null;
  motivation: string | null;
  impact: string | null;
}

export interface DiscoveryAnswers {
  typicalWeek?: string;
  feelings?: string[];
  doWithoutPay?: string;
  problemsNoticed?: string;
  admiredWork?: string;
  ifMoneyWerentFactor?: string;
  driftedAwayFrom?: string;
  drainingWork?: string;
  futureExperience?: string;
  whatWouldYouChange?: string;
  problemToSolve?: string;
  spendTimeDoing?: string;
  proudInFiveYears?: string;
  regretNeverTrying?: string;
  impactOnOthers?: string;
}

export interface DreamDirection {
  directionStatement: string;
  whyItFits: string;
  skillsPresent: string;
  skillsNeeded: string;
  opportunities: string;
  challenges: string;
  firstExperiment: string;
  suggestedBusinessShaped: boolean;
}

export async function getMyDream(): Promise<Dream | null> {
  const cookieStore = await cookies();
  const cookieHeader = cookieStore.toString();
  if (!cookieHeader) return null;

  try {
    const response = await fetch(`${API_BASE_URL}/api/v1/dreams/me`, {
      headers: { cookie: cookieHeader },
      cache: "no-store",
    });
    if (!response.ok) return null;
    return (await response.json()) as Dream;
  } catch {
    return null;
  }
}
