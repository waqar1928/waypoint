"use client";

import { useState } from "react";
import { Flag } from "lucide-react";
import { Button } from "@/components/ui/button";
import { apiMutate } from "@/lib/api-client";

const reasonLabels: Record<string, string> = {
  spam: "Spam",
  harassment: "Harassment",
  other: "Other",
};

/** Reusable across Community posts/comments and Mentorship help requests — files into the same
 * content_reports table via /api/community/reports regardless of entityType, matching the
 * backend's shared IContentReportSink port. */
export function ReportButton({ entityType, entityId }: { entityType: string; entityId: string }) {
  const [isOpen, setIsOpen] = useState(false);
  const [reason, setReason] = useState("spam");
  const [details, setDetails] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [status, setStatus] = useState<"idle" | "sent" | "error">("idle");

  if (status === "sent") {
    return <span className="text-xs text-ink-500">Reported. Thanks for flagging this.</span>;
  }

  if (!isOpen) {
    return (
      <button
        type="button"
        onClick={() => setIsOpen(true)}
        className="flex items-center gap-1 text-xs font-medium text-ink-500 hover:text-merlot-600"
      >
        <Flag className="h-3 w-3" aria-hidden="true" />
        Report
      </button>
    );
  }

  return (
    <form
      className="mt-2 space-y-2 rounded-[10px] border border-ink-300 bg-paper-raised p-3"
      onSubmit={async (e) => {
        e.preventDefault();
        setIsSubmitting(true);
        const response = await apiMutate("/api/community/reports", {
          method: "POST",
          headers: { "content-type": "application/json" },
          body: JSON.stringify({ entityType, entityId, reason, details: details || null }),
        });
        setIsSubmitting(false);
        setStatus(response.ok ? "sent" : "error");
      }}
    >
      <div className="flex items-center gap-2">
        <select
          value={reason}
          onChange={(e) => setReason(e.target.value)}
          aria-label="Reason for report"
          className="min-h-9 rounded-[8px] border border-ink-300 bg-paper-raised px-2 text-xs text-ink-900"
        >
          {Object.entries(reasonLabels).map(([value, label]) => (
            <option key={value} value={value}>
              {label}
            </option>
          ))}
        </select>
        <Button type="submit" variant="destructive" isLoading={isSubmitting} className="min-h-9 px-3 text-xs">
          Submit report
        </Button>
        <button type="button" onClick={() => setIsOpen(false)} className="text-xs text-ink-500">
          Cancel
        </button>
      </div>
      <input
        value={details}
        onChange={(e) => setDetails(e.target.value)}
        placeholder="Add detail (optional)"
        aria-label="Report details"
        className="min-h-9 w-full rounded-[8px] border border-ink-300 bg-paper-raised px-2 text-xs text-ink-900"
      />
      {status === "error" ? <p className="text-xs text-merlot-600">Couldn&rsquo;t send that report. Please try again.</p> : null}
    </form>
  );
}
