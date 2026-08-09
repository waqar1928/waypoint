import { getMyConversations, getConversationMessages } from "@/lib/coach";
import { CoachWorkspace } from "@/components/coach/coach-workspace";
import type { AiConversationTopic, Conversation } from "@/lib/coach";

export default async function CoachPage({
  searchParams,
}: {
  searchParams: Promise<{ start?: string; conversationId?: string }>;
}) {
  const params = await searchParams;
  const conversations = await getMyConversations();

  let initialConversation: Conversation | null = null;
  const requestedSummary = params.conversationId
    ? conversations.find((c) => c.id === params.conversationId)
    : undefined;
  if (requestedSummary) {
    const messages = await getConversationMessages(requestedSummary.id);
    initialConversation = { id: requestedSummary.id, topic: requestedSummary.topic, dreamId: null, messages };
  }

  const autoStartTopic = !initialConversation ? ((params.start as AiConversationTopic | undefined) ?? null) : null;

  return (
    <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">Waypoint Coach</h1>
      <p className="mt-2 text-ink-700">
        A collaborative guide, not a verdict — Coach asks questions and offers suggestions, but the
        decisions are always yours.
      </p>
      <div className="mt-8">
        <CoachWorkspace
          initialConversations={conversations}
          initialConversation={initialConversation}
          autoStartTopic={autoStartTopic}
        />
      </div>
    </div>
  );
}
