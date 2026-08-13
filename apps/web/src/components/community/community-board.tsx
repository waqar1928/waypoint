"use client";

import { useState } from "react";
import { PostComposer } from "@/components/community/post-composer";
import { PostCard } from "@/components/community/post-card";
import { apiMutate } from "@/lib/api-client";
import type { Post } from "@/lib/community";
import type { CreatePostInput } from "@/lib/validation";

export function CommunityBoard({ initialPosts }: { initialPosts: Post[] }) {
  const [posts, setPosts] = useState(initialPosts);
  const [error, setError] = useState<string | null>(null);

  const handleCreate = async (values: CreatePostInput) => {
    setError(null);
    const response = await apiMutate("/api/community/posts", {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(values),
    });
    if (!response.ok) {
      setError("We couldn't post that. Please try again.");
      return;
    }
    const created = (await response.json()) as Post;
    setPosts((current) => [created, ...current]);
  };

  const handleDelete = (postId: string) => {
    setPosts((current) => current.filter((p) => p.id !== postId));
  };

  return (
    <div className="space-y-4">
      <PostComposer onCreate={handleCreate} />

      {error ? (
        <p role="alert" className="text-sm text-merlot-600">
          {error}
        </p>
      ) : null}

      {posts.length === 0 ? (
        <p className="text-sm text-ink-500">
          No posts yet. Be the first to share something with the Drevia community.
        </p>
      ) : (
        <ul className="space-y-3">
          {posts.map((post) => (
            <li key={post.id}>
              <PostCard post={post} onDelete={handleDelete} />
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
