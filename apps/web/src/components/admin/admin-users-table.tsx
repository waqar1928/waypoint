"use client";

import { useState } from "react";
import { Lock, Unlock } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { apiMutate } from "@/lib/api-client";
import type { AdminUser } from "@/lib/admin";

export function AdminUsersTable({ initialUsers }: { initialUsers: AdminUser[] }) {
  const [users, setUsers] = useState(initialUsers);
  const [pendingId, setPendingId] = useState<string | null>(null);

  const toggleLock = async (user: AdminUser) => {
    setPendingId(user.id);
    const action = user.isLockedOut ? "unlock" : "lock";
    const response = await apiMutate(`/api/admin/users/${user.id}/${action}`, { method: "POST" });
    setPendingId(null);
    if (response.ok) {
      setUsers((prev) =>
        prev.map((u) => (u.id === user.id ? { ...u, isLockedOut: !u.isLockedOut, lockoutEnd: !u.isLockedOut ? "9999-12-31T23:59:59+00:00" : null } : u)),
      );
    }
  };

  if (users.length === 0) {
    return (
      <Card>
        <p className="text-sm text-ink-500">No users yet.</p>
      </Card>
    );
  }

  return (
    <Card className="p-0">
      <ul className="divide-y divide-ink-300">
        {users.map((user) => (
          <li key={user.id} className="flex items-center justify-between gap-3 p-4">
            <div className="min-w-0">
              <p className="truncate text-sm font-medium text-ink-900">{user.displayName ?? user.email}</p>
              <p className="truncate text-xs text-ink-500">
                {user.email}
                {user.isLockedOut ? (
                  <span className="ml-2 rounded-full bg-merlot-600/10 px-2 py-0.5 text-[11px] font-medium text-merlot-600">
                    Locked
                  </span>
                ) : null}
                {!user.emailConfirmed ? (
                  <span className="ml-2 rounded-full bg-ink-300/40 px-2 py-0.5 text-[11px] font-medium text-ink-700">
                    Unverified email
                  </span>
                ) : null}
              </p>
            </div>
            <Button
              variant={user.isLockedOut ? "secondary" : "destructive"}
              className="shrink-0 gap-2 px-3"
              isLoading={pendingId === user.id}
              onClick={() => toggleLock(user)}
            >
              {user.isLockedOut ? (
                <Unlock className="h-4 w-4" aria-hidden="true" />
              ) : (
                <Lock className="h-4 w-4" aria-hidden="true" />
              )}
              {user.isLockedOut ? "Unlock" : "Lock"}
            </Button>
          </li>
        ))}
      </ul>
    </Card>
  );
}
