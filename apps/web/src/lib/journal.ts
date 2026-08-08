import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";

export type JournalEntryType =
  | "daily"
  | "weekly"
  | "lesson"
  | "win"
  | "failure"
  | "idea"
  | "gratitude"
  | "vision";

export interface JournalEntry {
  id: string;
  entryType: JournalEntryType;
  body: string;
  createdAt: string;
}

export async function getRecentJournalEntries(): Promise<JournalEntry[]> {
  const cookieStore = await cookies();
  const cookieHeader = cookieStore.toString();
  if (!cookieHeader) return [];

  try {
    const response = await fetch(`${API_BASE_URL}/api/v1/journal`, {
      headers: { cookie: cookieHeader },
      cache: "no-store",
    });
    if (!response.ok) return [];
    return (await response.json()) as JournalEntry[];
  } catch {
    return [];
  }
}
