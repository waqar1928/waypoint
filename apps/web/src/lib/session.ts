import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";

export interface Session {
  userId: string;
  email: string;
  onboardingCompleted: boolean;
  isAdmin: boolean;
}

/** Server-side session lookup — reads the forwarded session cookie and asks the API directly. */
export async function getSession(): Promise<Session | null> {
  const cookieStore = await cookies();
  const cookieHeader = cookieStore.toString();
  if (!cookieHeader) return null;

  try {
    const response = await fetch(`${API_BASE_URL}/api/v1/auth/session`, {
      headers: { cookie: cookieHeader },
      cache: "no-store",
    });
    if (!response.ok) return null;
    return (await response.json()) as Session;
  } catch {
    return null;
  }
}
