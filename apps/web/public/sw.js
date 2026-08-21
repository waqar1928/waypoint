// Drevia's service worker exists for exactly one thing: Web Push. It does NOT implement a
// `fetch` handler and does NOT cache anything - per the approved P1 design, authenticated /app/*
// data stays out of any offline cache, and this app is not attempting to be offline-first in this
// phase. Do not add a fetch handler here without a fresh security review.
//
// skipWaiting()/clients.claim() are used so an updated service worker (e.g. a copy fix in the
// notification text below) takes effect on the next push rather than waiting for every open tab
// to be closed first - there's no meaningful cached state here that would make an in-flight old
// version harmful to keep briefly, so there's no reason to make users wait for it.

self.addEventListener("install", () => {
  self.skipWaiting();
});

self.addEventListener("activate", (event) => {
  event.waitUntil(self.clients.claim());
});

// The server (ScheduledNotificationWorker, via PushPayloadBuilder) already decided exactly what
// this notification says and whether it's the generic "Your next move is ready." or detailed
// content - this handler has no content-decision logic of its own. It only ever displays what
// it's given.
self.addEventListener("push", (event) => {
  let data = {};
  try {
    data = event.data ? event.data.json() : {};
  } catch {
    data = {};
  }

  const title = data.title || "Drevia";
  const body = data.body || "Your next move is ready.";
  const url = data.url || "/app/dashboard";

  event.waitUntil(
    self.registration.showNotification(title, {
      body,
      icon: "/android-chrome-192x192.png",
      badge: "/android-chrome-192x192.png",
      data: { url },
    }),
  );
});

self.addEventListener("notificationclick", (event) => {
  event.notification.close();
  const targetUrl = event.notification.data && event.notification.data.url ? event.notification.data.url : "/app/dashboard";

  event.waitUntil(
    self.clients.matchAll({ type: "window", includeUncontrolled: true }).then((clients) => {
      for (const client of clients) {
        // Focus an already-open Drevia tab rather than opening a new one, if one exists.
        if ("focus" in client) {
          client.navigate(targetUrl);
          return client.focus();
        }
      }
      return self.clients.openWindow(targetUrl);
    }),
  );
});
