import { CheckCircle2, XCircle } from "lucide-react";
import { Card } from "@/components/ui/card";
import { getAuditLog, getSystemHealth } from "@/lib/admin";

export default async function AdminSystemHealthPage() {
  const [health, auditLog] = await Promise.all([getSystemHealth(), getAuditLog()]);
  const isHealthy = health?.status === "Healthy";

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">System health</h1>
      <p className="mt-2 text-ink-700">Live backend status and the most recent audit trail.</p>

      <Card className="mt-6">
        <div className="flex items-center gap-3">
          {isHealthy ? (
            <CheckCircle2 className="h-5 w-5 text-sage-600" aria-hidden="true" />
          ) : (
            <XCircle className="h-5 w-5 text-merlot-600" aria-hidden="true" />
          )}
          <p className="text-sm font-medium text-ink-900">API: {health?.status ?? "Unknown"}</p>
        </div>
      </Card>

      <h2 className="mt-8 font-display text-lg font-semibold text-ink-900">Audit log</h2>
      <p className="mt-1 text-sm text-ink-700">Most recent {auditLog.length} events.</p>
      <div className="mt-3">
        {auditLog.length === 0 ? (
          <Card>
            <p className="text-sm text-ink-500">No audit events yet.</p>
          </Card>
        ) : (
          <Card className="p-0">
            <ul className="divide-y divide-ink-300">
              {auditLog.map((entry) => (
                <li key={entry.id} className="p-3 sm:p-4">
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <p className="text-sm font-medium text-ink-900">
                      {entry.entityType} · {entry.action}
                    </p>
                    <time className="text-xs text-ink-500" dateTime={entry.occurredAt}>
                      {new Date(entry.occurredAt).toLocaleString("en-US")}
                    </time>
                  </div>
                  {entry.payloadRedacted ? (
                    <p className="mt-1 text-xs text-ink-500">{entry.payloadRedacted}</p>
                  ) : null}
                </li>
              ))}
            </ul>
          </Card>
        )}
      </div>
    </div>
  );
}
