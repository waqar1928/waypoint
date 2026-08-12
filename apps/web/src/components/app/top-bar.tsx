"use client";

import { useRouter } from "next/navigation";
import { LogOut } from "lucide-react";
import { Button } from "@/components/ui/button";
import { NotificationBell } from "@/components/app/notification-bell";
import { apiMutate, invalidateCsrfToken } from "@/lib/api-client";

export function TopBar({ email }: { email: string }) {
  const router = useRouter();

  const handleLogout = async () => {
    await apiMutate("/api/auth/logout", { method: "POST" });
    invalidateCsrfToken();
    router.push("/login");
    router.refresh();
  };

  return (
    <header className="flex h-16 items-center justify-between border-b border-ink-300 bg-paper-raised px-4 sm:px-6">
      <p className="text-sm text-ink-700">
        Signed in as <span className="font-medium text-ink-900">{email}</span>
      </p>
      <div className="flex items-center gap-2">
        <NotificationBell />
        <Button variant="ghost" onClick={handleLogout} className="gap-2 px-3">
          <LogOut className="h-4 w-4" aria-hidden="true" />
          Log out
        </Button>
      </div>
    </header>
  );
}
