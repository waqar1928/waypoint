"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { Bell } from "lucide-react";
import { clsx } from "clsx";
import { apiMutate } from "@/lib/api-client";

interface NotificationDto {
  id: string;
  category: string;
  title: string;
  body: string;
  linkUrl: string | null;
  isRead: boolean;
  createdAt: string;
}

const POLL_INTERVAL_MS = 60_000;

function timeAgo(iso: string): string {
  const seconds = Math.max(0, Math.floor((Date.now() - new Date(iso).getTime()) / 1000));
  if (seconds < 60) return "just now";
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  return `${days}d ago`;
}

/**
 * Bell icon with an unread-count badge and a dropdown list — the only in-app surface for the
 * Notifications module (see docs/PRODUCTION_READINESS_AUDIT.md's Notifications module writeup).
 * Unread count polls on an interval rather than a persistent connection (SignalR/SSE): this
 * codebase has no real-time infrastructure anywhere else, and a 60s-stale badge is an acceptable
 * trade-off for not introducing the first one just for this.
 */
export function NotificationBell() {
  const router = useRouter();
  const [unreadCount, setUnreadCount] = useState(0);
  const [isOpen, setIsOpen] = useState(false);
  const [notifications, setNotifications] = useState<NotificationDto[] | null>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const refreshUnreadCount = useCallback(async () => {
    const response = await fetch("/api/notifications/unread-count");
    if (!response.ok) return;
    const { count } = (await response.json()) as { count: number };
    setUnreadCount(count);
  }, []);

  useEffect(() => {
    // Wrapped in an IIFE rather than calling refreshUnreadCount() directly — the linter's
    // react-hooks/set-state-in-effect rule flags synchronously invoking a function reference
    // known to call setState at the top of an effect body; wrapping it (matching the pattern
    // already used in the verify-email page's polling-on-mount effect) satisfies that without
    // changing the actual behavior — this still fires once on mount.
    void (async () => {
      await refreshUnreadCount();
    })();
    const interval = setInterval(refreshUnreadCount, POLL_INTERVAL_MS);
    return () => clearInterval(interval);
  }, [refreshUnreadCount]);

  useEffect(() => {
    if (!isOpen) return;

    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [isOpen]);

  const openDropdown = async () => {
    setIsOpen(true);
    const response = await fetch("/api/notifications");
    if (response.ok) {
      setNotifications((await response.json()) as NotificationDto[]);
    }
  };

  const handleSelect = async (notification: NotificationDto) => {
    if (!notification.isRead) {
      await apiMutate(`/api/notifications/${notification.id}/read`, { method: "POST" });
      setNotifications((prev) =>
        prev?.map((n) => (n.id === notification.id ? { ...n, isRead: true } : n)) ?? null,
      );
      setUnreadCount((count) => Math.max(0, count - 1));
    }
    setIsOpen(false);
    if (notification.linkUrl) {
      router.push(notification.linkUrl);
    }
  };

  const handleMarkAllRead = async () => {
    await apiMutate("/api/notifications/read-all", { method: "POST" });
    setNotifications((prev) => prev?.map((n) => ({ ...n, isRead: true })) ?? null);
    setUnreadCount(0);
  };

  return (
    <div ref={containerRef} className="relative">
      <button
        type="button"
        aria-haspopup="true"
        aria-expanded={isOpen}
        aria-label={unreadCount > 0 ? `Notifications, ${unreadCount} unread` : "Notifications"}
        onClick={() => (isOpen ? setIsOpen(false) : openDropdown())}
        className="relative inline-flex h-10 w-10 items-center justify-center rounded-[10px] text-ink-700 transition-colors hover:bg-ink-100 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
      >
        <Bell className="h-5 w-5" aria-hidden="true" />
        {unreadCount > 0 ? (
          <span className="absolute right-1 top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-merlot-600 px-1 text-[10px] font-medium text-white">
            {unreadCount > 99 ? "99+" : unreadCount}
          </span>
        ) : null}
      </button>

      {isOpen ? (
        <div
          role="menu"
          aria-label="Notifications"
          className="absolute right-0 z-20 mt-2 w-80 max-w-[calc(100vw-2rem)] rounded-[10px] border border-ink-300 bg-paper-raised shadow-lg"
        >
          <div className="flex items-center justify-between border-b border-ink-300 px-4 py-3">
            <p className="text-sm font-medium text-ink-900">Notifications</p>
            {notifications?.some((n) => !n.isRead) ? (
              <button
                type="button"
                onClick={handleMarkAllRead}
                className="text-xs font-medium text-beacon-600 hover:underline"
              >
                Mark all read
              </button>
            ) : null}
          </div>

          <div className="max-h-96 overflow-y-auto">
            {notifications === null ? (
              <p className="px-4 py-6 text-center text-sm text-ink-500">Loading…</p>
            ) : notifications.length === 0 ? (
              <p className="px-4 py-6 text-center text-sm text-ink-500">
                No notifications yet.
              </p>
            ) : (
              <ul>
                {notifications.map((notification) => (
                  <li key={notification.id}>
                    <button
                      type="button"
                      role="menuitem"
                      onClick={() => handleSelect(notification)}
                      className={clsx(
                        "flex w-full flex-col gap-0.5 border-b border-ink-100 px-4 py-3 text-left transition-colors hover:bg-ink-100 last:border-b-0",
                        !notification.isRead && "bg-beacon-50",
                      )}
                    >
                      <span className="flex items-center gap-2 text-sm font-medium text-ink-900">
                        {!notification.isRead ? (
                          <span
                            className="h-1.5 w-1.5 shrink-0 rounded-full bg-beacon-500"
                            aria-hidden="true"
                          />
                        ) : null}
                        {notification.title}
                      </span>
                      <span className="text-sm text-ink-700">{notification.body}</span>
                      <span className="text-xs text-ink-500">{timeAgo(notification.createdAt)}</span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      ) : null}
    </div>
  );
}
