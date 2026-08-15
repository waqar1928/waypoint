"use client";

import { useState } from "react";
import { clsx } from "clsx";
import { Compass } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ReportButton } from "@/components/shared/report-button";
import { apiMutate } from "@/lib/api-client";
import type { HelpRequest, HelpRequestResponse } from "@/lib/mentorship";

const statusStyles: Record<string, string> = {
  open: "bg-amber-100 text-amber-600",
  answered: "bg-sage-100 text-sage-600",
  closed: "bg-ink-100 text-ink-700",
};

export function HelpRequestCard({
  request,
  onUpdate,
}: {
  request: HelpRequest;
  onUpdate: (updated: HelpRequest) => void;
}) {
  const [responses, setResponses] = useState<HelpRequestResponse[] | null>(null);
  const [isLoadingResponses, setIsLoadingResponses] = useState(false);
  const [responseText, setResponseText] = useState("");
  const [isSending, setIsSending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadResponses = async () => {
    if (responses !== null) {
      setResponses(null);
      return;
    }
    setIsLoadingResponses(true);
    const response = await fetch(`/api/mentorship/help-requests/${request.id}/responses`);
    setIsLoadingResponses(false);
    if (response.ok) {
      setResponses((await response.json()) as HelpRequestResponse[]);
    }
  };

  const sendResponse = async () => {
    if (!responseText.trim()) return;
    setIsSending(true);
    setError(null);
    const response = await apiMutate(`/api/mentorship/help-requests/${request.id}/responses`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ body: responseText.trim() }),
    });
    setIsSending(false);
    if (!response.ok) {
      setError("We couldn't send that response. Please try again.");
      return;
    }
    const created = (await response.json()) as HelpRequestResponse;
    setResponses((current) => [...(current ?? []), created]);
    setResponseText("");
    onUpdate({ ...request, status: "answered", responseCount: request.responseCount + 1 });
  };

  const close = async () => {
    const response = await apiMutate(`/api/mentorship/help-requests/${request.id}/close`, { method: "POST" });
    if (response.ok) {
      onUpdate((await response.json()) as HelpRequest);
    }
  };

  return (
    <Card>
      <div className="flex items-start justify-between gap-3">
        <div>
          <span className="rounded-full bg-ink-100 px-2 py-0.5 text-xs font-medium text-ink-700">
            {request.category}
          </span>
          <h3 className="mt-2 font-display text-base font-semibold text-ink-900">{request.title}</h3>
          <p className="text-xs text-ink-500">by {request.author.displayName}</p>
        </div>
        <span className={clsx("rounded-full px-2 py-0.5 text-xs font-medium", statusStyles[request.status])}>
          {request.status}
        </span>
      </div>

      <p className="mt-2 whitespace-pre-wrap text-sm text-ink-700">{request.body}</p>

      {request.attachedDream ? (
        <div className="mt-3 flex items-start gap-2 rounded-[10px] bg-ink-100 px-3 py-2.5">
          <Compass className="mt-0.5 h-3.5 w-3.5 shrink-0 text-beacon-500" aria-hidden="true" />
          <div>
            <p className="text-xs font-medium text-ink-700">{request.attachedDream.title}</p>
            <p className="mt-0.5 text-xs text-ink-500">{request.attachedDream.statement}</p>
          </div>
        </div>
      ) : null}

      <div className="mt-3 flex items-center gap-4 border-t border-ink-300 pt-3">
        <button type="button" onClick={loadResponses} className="text-xs font-medium text-beacon-600 hover:underline">
          {isLoadingResponses ? "Loading…" : `${request.responseCount} ${request.responseCount === 1 ? "response" : "responses"}`}
        </button>
        {request.isMine && request.status !== "closed" ? (
          <button type="button" onClick={close} className="text-xs font-medium text-ink-500 hover:text-ink-900">
            Mark closed
          </button>
        ) : null}
        <ReportButton entityType="help_request" entityId={request.id} />
      </div>

      {responses !== null ? (
        <div className="mt-3 space-y-3 border-t border-ink-300 pt-3">
          {responses.length === 0 ? (
            <p className="text-xs text-ink-500">No responses yet.</p>
          ) : (
            responses.map((r) => (
              <div key={r.id} className="rounded-[10px] bg-ink-100 px-3 py-2">
                <p className="text-xs font-medium text-ink-900">{r.responder.displayName}</p>
                <p className="mt-0.5 text-sm text-ink-700">{r.body}</p>
              </div>
            ))
          )}

          {error ? (
            <p role="alert" className="text-xs text-merlot-600">
              {error}
            </p>
          ) : null}

          {request.status !== "closed" ? (
            <div className="flex gap-2">
              <input
                value={responseText}
                onChange={(e) => setResponseText(e.target.value)}
                placeholder="Offer some help…"
                aria-label="Response"
                className="min-h-9 flex-1 rounded-[8px] border border-ink-300 bg-paper-raised px-3 text-sm text-ink-900"
              />
              <Button onClick={sendResponse} isLoading={isSending} className="min-h-9 px-3 text-xs">
                Send
              </Button>
            </div>
          ) : null}
        </div>
      ) : null}
    </Card>
  );
}
