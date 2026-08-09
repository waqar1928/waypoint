import { getAdminUsers } from "@/lib/admin";
import { AdminUsersTable } from "@/components/admin/admin-users-table";

export default async function AdminUsersPage() {
  const users = await getAdminUsers();

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Users</h1>
      <p className="mt-2 text-ink-700">{users.length} registered {users.length === 1 ? "user" : "users"}.</p>

      <div className="mt-6">
        <AdminUsersTable initialUsers={users} />
      </div>
    </div>
  );
}
