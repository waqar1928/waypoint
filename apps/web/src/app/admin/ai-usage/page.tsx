import { Card } from "@/components/ui/card";
import { getAiUsageSummary } from "@/lib/admin";

const topicLabels: Record<string, string> = {
  coach: "Drevia Coach",
  dreamAnalysis: "Dream Analysis",
  challengeMyIdea: "Challenge My Idea",
};

export default async function AdminAiUsagePage() {
  const usage = await getAiUsageSummary();

  return (
    <div className="mx-auto max-w-4xl px-4 py-8 sm:px-6 sm:py-10 lg:px-10">
      <h1 className="font-display text-3xl font-semibold text-ink-900">AI usage</h1>
      <p className="mt-2 text-ink-700">Conversation and token volume across every AI feature.</p>

      {!usage ? (
        <Card className="mt-6">
          <p className="text-sm text-ink-500">Usage data unavailable.</p>
        </Card>
      ) : (
        <>
          <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-3">
            <Card>
              <p className="font-display text-2xl font-semibold text-ink-900">{usage.totalConversations}</p>
              <p className="mt-1 text-sm text-ink-700">Conversations</p>
            </Card>
            <Card>
              <p className="font-display text-2xl font-semibold text-ink-900">{usage.totalMessages}</p>
              <p className="mt-1 text-sm text-ink-700">Messages</p>
            </Card>
            <Card>
              <p className="font-display text-2xl font-semibold text-ink-900">{usage.totalTokens.toLocaleString()}</p>
              <p className="mt-1 text-sm text-ink-700">Tokens</p>
            </Card>
          </div>

          <h2 className="mt-8 font-display text-lg font-semibold text-ink-900">By topic</h2>
          <div className="mt-3">
            {usage.byTopic.length === 0 ? (
              <Card>
                <p className="text-sm text-ink-500">No AI activity yet.</p>
              </Card>
            ) : (
              <Card className="p-0">
                <ul className="divide-y divide-ink-300">
                  {usage.byTopic.map((topic) => (
                    <li key={topic.topic} className="flex items-center justify-between gap-3 p-4">
                      <p className="text-sm font-medium text-ink-900">{topicLabels[topic.topic] ?? topic.topic}</p>
                      <p className="text-xs text-ink-500">
                        {topic.conversationCount} conv · {topic.messageCount} msgs · {topic.totalTokens.toLocaleString()} tokens
                      </p>
                    </li>
                  ))}
                </ul>
              </Card>
            )}
          </div>
        </>
      )}
    </div>
  );
}
