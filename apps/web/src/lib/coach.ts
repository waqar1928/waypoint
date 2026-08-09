import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";

export type AiConversationTopic = "coach" | "dreamAnalysis" | "challengeMyIdea";
export type AiMessageRole = "user" | "assistant" | "system";

export interface ConversationSummary {
  id: string;
  topic: AiConversationTopic;
  updatedAt: string;
}

export interface Message {
  id: string;
  role: AiMessageRole;
  content: string;
  createdAt: string;
}

export interface Conversation {
  id: string;
  topic: AiConversationTopic;
  dreamId: string | null;
  messages: Message[];
}

async function serverFetch<T>(path: string): Promise<T | null> {
  const cookieStore = await cookies();
  const cookieHeader = cookieStore.toString();
  if (!cookieHeader) return null;

  try {
    const response = await fetch(`${API_BASE_URL}${path}`, {
      headers: { cookie: cookieHeader },
      cache: "no-store",
    });
    if (!response.ok) return null;
    return (await response.json()) as T;
  } catch {
    return null;
  }
}

export async function getMyConversations(): Promise<ConversationSummary[]> {
  return (await serverFetch<ConversationSummary[]>("/api/v1/ai/conversations")) ?? [];
}

export async function getConversationMessages(conversationId: string): Promise<Message[]> {
  return (await serverFetch<Message[]>(`/api/v1/ai/conversations/${conversationId}/messages`)) ?? [];
}
