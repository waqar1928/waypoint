import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";
import type { VisibilityLevel } from "@/lib/validation";

export type { VisibilityLevel };

export interface PrivacySettings {
  profileVisibility: VisibilityLevel;
  dreamVisibility: VisibilityLevel;
}

export async function getPrivacySettings(): Promise<PrivacySettings | null> {
  const cookieStore = await cookies();
  const cookieHeader = cookieStore.toString();
  if (!cookieHeader) return null;

  try {
    const response = await fetch(`${API_BASE_URL}/api/v1/me/privacy-settings`, {
      headers: { cookie: cookieHeader },
      cache: "no-store",
    });
    if (!response.ok) return null;
    return (await response.json()) as PrivacySettings;
  } catch {
    return null;
  }
}
