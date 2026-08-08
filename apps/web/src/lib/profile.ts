import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";

export interface Profile {
  userId: string;
  displayName: string;
  bio: string | null;
  avatarUrl: string | null;
  timeZone: string;
  locale: string;
  onboardingCompletedAt: string | null;
}

export async function getProfile(): Promise<Profile | null> {
  const cookieStore = await cookies();
  const cookieHeader = cookieStore.toString();
  if (!cookieHeader) return null;

  try {
    const response = await fetch(`${API_BASE_URL}/api/v1/me/profile`, {
      headers: { cookie: cookieHeader },
      cache: "no-store",
    });
    if (!response.ok) return null;
    return (await response.json()) as Profile;
  } catch {
    return null;
  }
}
