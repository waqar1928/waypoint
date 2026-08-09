import { cookies } from "next/headers";
import { API_BASE_URL } from "@/lib/api-config";

export type PostVisibility = "private" | "community" | "public";

export interface Author {
  userId: string;
  displayName: string;
  avatarUrl: string | null;
}

export interface Post {
  id: string;
  author: Author;
  body: string;
  visibility: PostVisibility;
  commentCount: number;
  isMine: boolean;
  createdAt: string;
}

export interface Comment {
  id: string;
  author: Author;
  body: string;
  isMine: boolean;
  createdAt: string;
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

export async function getCommunityFeed(): Promise<Post[]> {
  return (await serverFetch<Post[]>("/api/v1/community/feed")) ?? [];
}

export async function getMyPosts(): Promise<Post[]> {
  return (await serverFetch<Post[]>("/api/v1/community/posts/mine")) ?? [];
}
