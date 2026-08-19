"use client";

import { useEffect, useRef, useState } from "react";
import { clsx } from "clsx";
import { Button } from "@/components/ui/button";
import { apiMutate } from "@/lib/api-client";
import type { AiConversationTopic, Conversation, ConversationSummary, Message } from "@/lib/coach";

const topicLabels: Record<AiConversationTopic, string> = {
  coach: "General",
  dreamAnalysis: "Dream analysis",
  challengeMyIdea: "Challenge my idea",
};

export function CoachWorkspace({
  initialConversations,
  initialConversation,
  autoStartTopic,
}: {
  initialConversations: ConversationSummary[];
  initialConversation: Conversation | null;
  autoStartTopic: AiConversationTopic | null;
}) {
  const [conversations, setConversations] = useState(initialConversations);
  const [active, setActive] = useState<Conversation | null>(initialConversation);
  const [input, setInput] = useState("");
  const [isSending, setIsSending] = useState(false);
  const [isStarting, setIsStarting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  // Opt-in per conversation, not a saved preference - the user decides fresh each time. Defaults
  // to off so a normal "New conversation" click behaves exactly as it always has.
  const [includeProgressContext, setIncludeProgressContext] = useState(false);
  const hasAutoStarted = useRef(false);
  const threadEndRef = useRef<HTMLDivElement>(null);

  const startConversation = async (topic: AiConversationTopic, includeProgress = false) => {
    setIsStarting(true);
    setError(null);
    const response = await apiMutate("/api/ai/conversations", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ topic, includeProgressContext: includeProgress }),
    });
    setIsStarting(false);

    if (!response.ok) {
      setError(await friendlyError(response));
      return;
    }

    const conversation = (await response.json()) as Conversation;
    setActive(conversation);
    setConversations((current) => [
      { id: conversation.id, topic: conversation.topic, updatedAt: new Date().toISOString() },
      ...current.filter((c) => c.id !== conversation.id),
    ]);
  };

  const openConversation = async (summary: ConversationSummary) => {
    setError(null);
    const response = await fetch(`/api/ai/conversations/${summary.id}/messages`);
    if (!response.ok) {
      setError("We couldn't load that conversation. Please try again.");
      return;
    }
    const messages = (await response.json()) as Message[];
    setActive({ id: summary.id, topic: summary.topic, dreamId: null, messages });
  };

  const sendMessage = async () => {
    if (!active || !input.trim() || isSending) return;
    const content = input.trim();
    const conversationId = active.id;
    setInput("");
    setIsSending(true);
    setError(null);

    setActive((current) =>
      current
        ? {
            ...current,
            messages: [
              ...current.messages,
              { id: `pending-${Date.now()}`, role: "user", content, createdAt: new Date().toISOString() },
            ],
          }
        : current,
    );

    const response = await apiMutate(`/api/ai/conversations/${conversationId}/messages`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ content }),
    });
    setIsSending(false);

    if (!response.ok) {
      setError(await friendlyError(response));
      return;
    }

    const assistantMessage = (await response.json()) as Message;
    setActive((current) =>
      current && current.id === conversationId
        ? { ...current, messages: [...current.messages, assistantMessage] }
        : current,
    );
  };

  useEffect(() => {
    if (autoStartTopic && !active && !hasAutoStarted.current) {
      hasAutoStarted.current = true;
      void startConversation(autoStartTopic);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [autoStartTopic]);

  useEffect(() => {
    threadEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [active?.messages.length]);

  return (
    <div className="grid gap-6 lg:grid-cols-[240px_1fr]">
      <div className="space-y-3">
        <Button
          onClick={() => startConversation("coach", includeProgressContext)}
          isLoading={isStarting}
          className="w-full"
        >
          New conversation
        </Button>
        {/* Opt-in, off by default, decided fresh per conversation - see the state comment above.
            Only affects the general Coach topic (this button); Dream Analysis and Challenge My
            Idea already have their own fixed, single-purpose context and ignore this flag even if
            it were sent, per StartConversationCommandHandler. */}
        <label className="flex items-start gap-2 text-xs text-ink-500">
          <input
            type="checkbox"
            checked={includeProgressContext}
            onChange={(e) => setIncludeProgressContext(e.target.checked)}
            className="mt-0.5 h-3.5 w-3.5 rounded border-ink-300"
          />
          <span>Let Coach see my recent actions, experiments, and learnings, not just my Dream</span>
        </label>
        {conversations.length > 0 ? (
          <ul className="space-y-1.5">
            {conversations.map((c) => (
              <li key={c.id}>
                <button
                  type="button"
                  onClick={() => openConversation(c)}
                  className={clsx(
                    "w-full rounded-[10px] px-3 py-2 text-left text-sm transition-colors",
                    active?.id === c.id
                      ? "bg-beacon-100 font-medium text-beacon-700"
                      : "text-ink-700 hover:bg-ink-100",
                  )}
                >
                  {topicLabels[c.topic]}
                </button>
              </li>
            ))}
          </ul>
        ) : (
          <p className="text-sm text-ink-500">No conversations yet.</p>
        )}
      </div>

      <div className="flex min-h-[480px] flex-col rounded-2xl border border-ink-300 bg-paper-raised">
        {error ? (
          <p role="alert" className="border-b border-ink-300 px-4 py-2.5 text-sm text-merlot-600">
            {error}
          </p>
        ) : null}

        {!active ? (
          <div className="flex flex-1 items-center justify-center p-8 text-center text-sm text-ink-500">
            Start a conversation with Drevia Coach, a collaborative guide, not a verdict.
          </div>
        ) : (
          <>
            <div className="flex-1 space-y-4 overflow-y-auto p-4">
              {active.messages.map((m) => (
                <div key={m.id} className={clsx("flex", m.role === "user" ? "justify-end" : "justify-start")}>
                  <div
                    className={clsx(
                      "max-w-[75%] whitespace-pre-wrap rounded-2xl px-4 py-2.5 text-sm",
                      m.role === "user" ? "bg-beacon-500 text-white" : "bg-ink-100 text-ink-900",
                    )}
                  >
                    {m.content}
                  </div>
                </div>
              ))}
              {isSending ? (
                <div className="flex justify-start">
                  <div className="max-w-[75%] rounded-2xl bg-ink-100 px-4 py-2.5 text-sm text-ink-500">
                    Coach is thinking…
                  </div>
                </div>
              ) : null}
              <div ref={threadEndRef} />
            </div>

            <form
              className="flex gap-2 border-t border-ink-300 p-3"
              onSubmit={(e) => {
                e.preventDefault();
                void sendMessage();
              }}
            >
              <input
                value={input}
                onChange={(e) => setInput(e.target.value)}
                placeholder="Type a message…"
                aria-label="Message"
                className="min-h-11 flex-1 rounded-[10px] border border-ink-300 bg-paper-raised px-3.5 text-sm text-ink-900 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2"
              />
              <Button type="submit" isLoading={isSending} disabled={!input.trim()}>
                Send
              </Button>
            </form>
          </>
        )}
      </div>
    </div>
  );
}

async function friendlyError(response: Response): Promise<string> {
  try {
    const problem = (await response.json()) as { detail?: string };
    return problem.detail ?? "Something went wrong. Please try again.";
  } catch {
    return "Something went wrong. Please try again.";
  }
}
