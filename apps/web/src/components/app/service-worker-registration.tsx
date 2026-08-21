"use client";

import { useEffect } from "react";
import { registerServiceWorker } from "@/lib/push-notifications";

/**
 * Passively registers /sw.js on every authenticated page load - renders nothing, has no visible
 * effect, and never requests Notification permission (see push-notifications.ts's doc comment).
 * This just makes the service worker ready so the "Enable push notifications" button in Settings
 * doesn't have to register-and-wait the first time someone clicks it.
 */
export function ServiceWorkerRegistration() {
  useEffect(() => {
    void registerServiceWorker();
  }, []);

  return null;
}
