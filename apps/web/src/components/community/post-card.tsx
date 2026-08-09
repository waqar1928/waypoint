"use client";

import { useState } from "react";
import { clsx } from "clsx";
import { Lock, Users } from "lucide-react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ReportButton } from "@/components/shared/report-button";
import { apiMutate } from "@/lib/api-client";
import type { Comment, Post } from "@/lib/community";

const visibilityIcon = { private: Lock, community: Users, public: Users } as const;
const visibilityLabel = { private: "Only me", community: "Community", public: "Public" } as const;

export function PostCard({ post, onDelete }: { post: Post; onDelete: (postId: string) => void }) {
  const [comments, setComments] = useState<Comment[] | null>(null);
  const [isLoadingComments, setIsLoadingComments] = useState(false);
  const [commentText, setCommentText] = useState("");
  const [isSendingComment, setIsSendingComment] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const VisIcon = visibilityIcon[post.visibility];
  const commentCount = comments?.length ?? post.commentCount;

  const loadComments = async () => {
    if (comments !== null) {
      setComments(null);
      return;
    }
    setIsLoadingComments(true);
    const response = await fetch(`/api/community/posts/${post.id}/comments`);
    setIsLoadingComments(false);
    if (response.ok) {
      setComments((await response.json()) as Comment[]);
    }
  };

  const sendComment = async () => {
    if (!commentText.trim()) return;
    setIsSendingComment(true);
    setError(null);
    const response = await apiMutate(`/api/community/posts/${post.id}/comments`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify({ body: commentText.trim() }),
    });
    setIsSendingComment(false);
    if (!response.ok) {
      setError("We couldn't add that comment. Please try again.");
      return;
    }
    const created = (await response.json()) as Comment;
    setComments((current) => [...(current ?? []), created]);
    setCommentText("");
  };

  const deletePost = async () => {
    const response = await apiMutate(`/api/community/posts/${post.id}`, { method: "DELETE" });
    if (response.ok) {
      onDelete(post.id);
    }
  };

  return (
    <Card>
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-center gap-2">
          <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-beacon-100 text-sm font-semibold text-beacon-700">
            {post.author.displayName.charAt(0).toUpperCase()}
          </div>
          <div>
            <p className="text-sm font-medium text-ink-900">{post.author.displayName}</p>
            <p className="flex items-center gap-1 text-xs text-ink-500">
              <VisIcon className="h-3 w-3" aria-hidden="true" />
              {visibilityLabel[post.visibility]}
            </p>
          </div>
        </div>
        {post.isMine ? (
          <button type="button" onClick={deletePost} className="text-xs text-ink-500 hover:text-merlot-600">
            Delete
          </button>
        ) : null}
      </div>

      <p className="mt-3 whitespace-pre-wrap text-sm text-ink-900">{post.body}</p>

      <div className="mt-3 flex items-center gap-4 border-t border-ink-300 pt-3">
        <button type="button" onClick={loadComments} className="text-xs font-medium text-beacon-600 hover:underline">
          {isLoadingComments
            ? "Loading…"
            : `${commentCount} ${commentCount === 1 ? "comment" : "comments"}`}
        </button>
        <ReportButton entityType="post" entityId={post.id} />
      </div>

      {comments !== null ? (
        <div className="mt-3 space-y-3 border-t border-ink-300 pt-3">
          {comments.length === 0 ? (
            <p className="text-xs text-ink-500">No comments yet.</p>
          ) : (
            comments.map((c) => (
              <div key={c.id} className={clsx("rounded-[10px] bg-ink-100 px-3 py-2")}>
                <p className="text-xs font-medium text-ink-900">{c.author.displayName}</p>
                <p className="mt-0.5 text-sm text-ink-700">{c.body}</p>
                <div className="mt-1">
                  <ReportButton entityType="comment" entityId={c.id} />
                </div>
              </div>
            ))
          )}

          {error ? (
            <p role="alert" className="text-xs text-merlot-600">
              {error}
            </p>
          ) : null}

          <div className="flex gap-2">
            <input
              value={commentText}
              onChange={(e) => setCommentText(e.target.value)}
              placeholder="Add a comment…"
              aria-label="Comment"
              className="min-h-9 flex-1 rounded-[8px] border border-ink-300 bg-paper-raised px-3 text-sm text-ink-900"
            />
            <Button onClick={sendComment} isLoading={isSendingComment} className="min-h-9 px-3 text-xs">
              Send
            </Button>
          </div>
        </div>
      ) : null}
    </Card>
  );
}
