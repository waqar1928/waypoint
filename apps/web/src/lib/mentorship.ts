import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";

export type VerificationStatus = "unverified" | "pending" | "verified";
export type HelpRequestCategory =
  | "business" | "marketing" | "technology" | "finance" | "sales" | "design" | "career" | "operations" | "leadership";
export type HelpRequestStatus = "open" | "answered" | "closed";

export interface Person {
  userId: string;
  displayName: string;
  avatarUrl: string | null;
}

export interface MentorProfile {
  id: string;
  mentor: Person;
  expertise: string[];
  yearsExperience: number | null;
  availability: string | null;
  verificationStatus: VerificationStatus;
}

/** Lean by design - title/statement only, matching the backend's AttachedDreamDto. Present only
 * when the author opted in to attaching their Dream (see help-requests-board.tsx). */
export interface AttachedDream {
  title: string;
  statement: string;
}

export interface HelpRequest {
  id: string;
  author: Person;
  dreamId: string | null;
  attachedDream: AttachedDream | null;
  category: HelpRequestCategory;
  title: string;
  body: string;
  status: HelpRequestStatus;
  responseCount: number;
  isMine: boolean;
  createdAt: string;
}

export interface HelpRequestResponse {
  id: string;
  responder: Person;
  body: string;
  isMine: boolean;
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

export async function getMentorDirectory(): Promise<MentorProfile[]> {
  return (await serverFetch<MentorProfile[]>("/api/v1/mentorship/mentors")) ?? [];
}

export async function getMyMentorProfile(): Promise<MentorProfile | null> {
  return serverFetch<MentorProfile>("/api/v1/mentorship/mentors/me");
}

export async function getHelpRequests(): Promise<HelpRequest[]> {
  return (await serverFetch<HelpRequest[]>("/api/v1/mentorship/help-requests")) ?? [];
}

export async function getMyHelpRequests(): Promise<HelpRequest[]> {
  return (await serverFetch<HelpRequest[]>("/api/v1/mentorship/help-requests/mine")) ?? [];
}
