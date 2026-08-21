import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";

export interface NotificationPreferences {
  emailProductUpdates: boolean;
  emailCoachNudges: boolean;
  emailCommunityActivity: boolean;
  pushEnabled: boolean;
  pushDetailedContent: boolean;
  pushDailyReminderEnabled: boolean;
  /** "HH:mm:ss" or null - see QuietHoursEvaluator on the backend for exact semantics. */
  quietHoursStart: string | null;
  quietHoursEnd: string | null;
}

export async function getNotificationPreferences(): Promise<NotificationPreferences | null> {
  const cookieStore = await cookies();
  const cookieHeader = cookieStore.toString();
  if (!cookieHeader) return null;

  try {
    const response = await fetch(`${API_BASE_URL}/api/v1/me/notification-preferences`, {
      headers: { cookie: cookieHeader },
      cache: "no-store",
    });
    if (!response.ok) return null;
    return (await response.json()) as NotificationPreferences;
  } catch {
    return null;
  }
}
