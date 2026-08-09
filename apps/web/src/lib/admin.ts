import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";
import type { MentorProfile } from "@/lib/mentorship";

export type DreamStage = "discover" | "define" | "validate" | "plan" | "act" | "learn" | "grow";
export type ReportStatus = "open" | "dismissed" | "contentRemoved" | "resolved";
export type AiConversationTopic = "coach" | "dreamAnalysis" | "challengeMyIdea";
export type VerificationStatus = "unverified" | "pending" | "verified";

export interface AdminUser {
  id: string;
  email: string;
  displayName: string | null;
  emailConfirmed: boolean;
  isLockedOut: boolean;
  lockoutEnd: string | null;
}

export interface AdminDream {
  id: string;
  userId: string;
  ownerDisplayName: string | null;
  title: string;
  stage: DreamStage;
  isBusinessShaped: boolean;
  createdAt: string;
}

export interface ModerationReport {
  id: string;
  entityType: string;
  entityId: string;
  reason: string;
  details: string | null;
  status: ReportStatus;
  reporterUserId: string;
  contentPreview: string | null;
  createdAt: string;
}

export interface TopicUsage {
  topic: AiConversationTopic;
  conversationCount: number;
  messageCount: number;
  totalTokens: number;
}

export interface AiUsageSummary {
  totalConversations: number;
  totalMessages: number;
  totalTokens: number;
  byTopic: TopicUsage[];
}

export interface AuditLogEntry {
  id: string;
  entityType: string;
  entityId: string;
  action: string;
  actorUserId: string | null;
  payloadRedacted: string | null;
  occurredAt: string;
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

export async function getAdminUsers(): Promise<AdminUser[]> {
  return (await serverFetch<AdminUser[]>("/api/v1/admin/users")) ?? [];
}

export async function getAdminDreams(): Promise<AdminDream[]> {
  return (await serverFetch<AdminDream[]>("/api/v1/admin/dreams")) ?? [];
}

export async function getModerationQueue(): Promise<ModerationReport[]> {
  return (await serverFetch<ModerationReport[]>("/api/v1/admin/moderation")) ?? [];
}

export async function getAdminMentors(): Promise<MentorProfile[]> {
  return (await serverFetch<MentorProfile[]>("/api/v1/admin/mentors")) ?? [];
}

export async function getAiUsageSummary(): Promise<AiUsageSummary | null> {
  return serverFetch<AiUsageSummary>("/api/v1/admin/ai-usage");
}

export async function getAuditLog(): Promise<AuditLogEntry[]> {
  return (await serverFetch<AuditLogEntry[]>("/api/v1/admin/audit-log")) ?? [];
}

export interface SystemHealth {
  status: string;
}

export async function getSystemHealth(): Promise<SystemHealth | null> {
  const cookieStore = await cookies();
  try {
    const response = await fetch(`${API_BASE_URL}/health/ready`, {
      headers: { cookie: cookieStore.toString() },
      cache: "no-store",
    });
    return { status: response.ok ? "Healthy" : `Unhealthy (${response.status})` };
  } catch {
    return { status: "Unreachable" };
  }
}
