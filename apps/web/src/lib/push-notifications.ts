import { apiMutate } from "@/lib/api-client";

/**
 * Pure browser-API helpers for Web Push - no React, so support-detection is trivially reusable
 * and testable independent of any component. Nothing here requests Notification permission on
 * its own load/import; permission is only ever requested from inside enablePushNotifications(),
 * which every caller must invoke directly from a genuine user action (a button's onClick) - see
 * PushNotificationSettings, the only caller.
 */

export type PushUnsupportedReason = "no-service-worker" | "no-push-manager" | "ios-not-installed";

export interface PushSupport {
  supported: boolean;
  reason: PushUnsupportedReason | null;
}

/** iOS Safari only supports Web Push inside an installed (Add to Home Screen) PWA, since iOS
 * 16.4 - a regular Safari tab can never subscribe no matter what the Notification/PushManager
 * globals report. There's no official "is this iOS Safari, not installed" API; the display-mode
 * media query (and navigator.standalone as an iOS-specific fallback) is the best available
 * signal. */
function isIos(): boolean {
  return /iphone|ipad|ipod/i.test(navigator.userAgent);
}

function isStandaloneDisplayMode(): boolean {
  const standaloneNavigator = navigator as Navigator & { standalone?: boolean };
  return Boolean(window.matchMedia?.("(display-mode: standalone)").matches || standaloneNavigator.standalone);
}

export function detectPushSupport(): PushSupport {
  if (typeof window === "undefined" || !("serviceWorker" in navigator)) {
    return { supported: false, reason: "no-service-worker" };
  }
  if (!("PushManager" in window)) {
    return { supported: false, reason: "no-push-manager" };
  }
  if (isIos() && !isStandaloneDisplayMode()) {
    return { supported: false, reason: "ios-not-installed" };
  }
  return { supported: true, reason: null };
}

function urlBase64ToUint8Array(base64String: string): Uint8Array {
  const padding = "=".repeat((4 - (base64String.length % 4)) % 4);
  const base64 = (base64String + padding).replace(/-/g, "+").replace(/_/g, "/");
  const rawData = window.atob(base64);
  const outputArray = new Uint8Array(rawData.length);
  for (let i = 0; i < rawData.length; i++) {
    outputArray[i] = rawData.charCodeAt(i);
  }
  return outputArray;
}

/** Passive - just makes the service worker file active so it's ready when (if) the user later
 * clicks "Enable push notifications." Does not request permission and has no visible effect. Safe
 * to call unconditionally on every authenticated page load. */
export async function registerServiceWorker(): Promise<void> {
  if (!("serviceWorker" in navigator)) return;
  try {
    await navigator.serviceWorker.register("/sw.js");
  } catch {
    // Non-critical - the "Enable push notifications" button re-attempts registration itself
    // (via navigator.serviceWorker.ready) when actually clicked.
  }
}

/** The explicit, user-gesture-triggered flow: request permission, subscribe, tell the server.
 * Throws a short, user-facing message on failure - never a raw browser error - so the calling
 * component can show something sensible without knowing Push API internals. */
export async function enablePushNotifications(): Promise<{ subscriptionId: string }> {
  const permission = await Notification.requestPermission();
  if (permission !== "granted") {
    throw new Error("Notification permission was not granted.");
  }

  await registerServiceWorker();
  const registration = await navigator.serviceWorker.ready;

  const keyResponse = await fetch("/api/notifications/push-public-key");
  if (!keyResponse.ok) {
    throw new Error("Push notifications aren't available right now.");
  }
  const { publicKey } = (await keyResponse.json()) as { publicKey: string };

  const subscription = await registration.pushManager.subscribe({
    userVisibleOnly: true,
    // TS's lib.dom types for PushManager.subscribe want an ArrayBuffer-backed BufferSource
    // specifically, which Uint8Array's generic type doesn't structurally satisfy on current
    // TypeScript/lib.dom versions even though this is exactly the shape the real Push API expects
    // at runtime - a well-known lib.dom.d.ts strictness quirk, not a real type mismatch.
    applicationServerKey: urlBase64ToUint8Array(publicKey) as BufferSource,
  });

  const json = subscription.toJSON();
  const response = await apiMutate("/api/notifications/push-subscriptions", {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify({
      endpoint: json.endpoint,
      keys: { p256dh: json.keys?.p256dh, auth: json.keys?.auth },
      userAgent: navigator.userAgent,
    }),
  });

  if (!response.ok) {
    // Roll back the browser-side subscription rather than leave the device "subscribed" from the
    // browser's own point of view while the server never learned about it.
    await subscription.unsubscribe();
    throw new Error("We couldn't save your subscription. Please try again.");
  }

  const saved = (await response.json()) as { id: string };
  return { subscriptionId: saved.id };
}

/** Unsubscribes the current browser locally (best-effort, always attempted even if there's no
 * known server-side id) - the caller is responsible for also telling the server via DELETE
 * /api/notifications/push-subscriptions/{id} if it knows the id (see PushNotificationSettings). */
export async function disablePushNotificationsLocally(): Promise<void> {
  if (!("serviceWorker" in navigator)) return;
  const registration = await navigator.serviceWorker.getRegistration();
  const subscription = await registration?.pushManager.getSubscription();
  if (subscription) {
    await subscription.unsubscribe();
  }
}
