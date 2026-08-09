"use client";

import { useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { apiMutate } from "@/lib/api-client";
import type { ModerationReport } from "@/lib/admin";

const entityTypeLabels: Record<string, string> = {
  post: "Post",
  comment: "Comment",
  help_request: "Help request",
};

export function ModerationQueue({ initialReports }: { initialReports: ModerationReport[] }) {
  const [reports, setReports] = useState(initialReports);
  const [pendingId, setPendingId] = useState<string | null>(null);

  const act = async (reportId: string, action: "dismiss" | "remove-content" | "resolve") => {
    setPendingId(reportId);
    const response = await apiMutate(`/api/admin/moderation/${reportId}/${action}`, { method: "POST" });
    setPendingId(null);
    if (response.ok) {
      setReports((prev) => prev.filter((r) => r.id !== reportId));
    }
  };

  if (reports.length === 0) {
    return (
      <Card>
        <p className="text-sm text-ink-500">No open reports. The queue is clear.</p>
      </Card>
    );
  }

  return (
    <ul className="space-y-4">
      {reports.map((report) => {
        const canRemove = report.entityType === "post" || report.entityType === "comment";
        return (
          <li key={report.id}>
            <Card>
              <div className="flex items-center justify-between gap-2">
                <span className="rounded-full bg-ink-100 px-2 py-0.5 text-[11px] font-medium text-ink-700">
                  {entityTypeLabels[report.entityType] ?? report.entityType}
                </span>
                <time className="text-xs text-ink-500" dateTime={report.createdAt}>
                  {new Date(report.createdAt).toLocaleString("en-US")}
                </time>
              </div>

              <p className="mt-3 text-sm font-medium text-ink-900">Reason: {report.reason}</p>
              {report.details ? <p className="mt-1 text-sm text-ink-700">{report.details}</p> : null}

              {report.contentPreview ? (
                <blockquote className="mt-3 rounded-[10px] border border-ink-300 bg-paper p-3 text-sm text-ink-700">
                  {report.contentPreview}
                </blockquote>
              ) : (
                <p className="mt-3 text-sm text-ink-500">No preview available for this content type.</p>
              )}

              <div className="mt-4 flex flex-wrap gap-2">
                <Button
                  variant="secondary"
                  isLoading={pendingId === report.id}
                  onClick={() => act(report.id, "dismiss")}
                >
                  Dismiss
                </Button>
                {canRemove ? (
                  <Button
                    variant="destructive"
                    isLoading={pendingId === report.id}
                    onClick={() => act(report.id, "remove-content")}
                  >
                    Remove content
                  </Button>
                ) : (
                  <Button
                    variant="secondary"
                    isLoading={pendingId === report.id}
                    onClick={() => act(report.id, "resolve")}
                  >
                    Resolve
                  </Button>
                )}
              </div>
            </Card>
          </li>
        );
      })}
    </ul>
  );
}
