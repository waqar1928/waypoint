import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function GET(request: NextRequest, { params }: { params: Promise<{ postId: string }> }) {
  const { postId } = await params;
  return proxyToApi(request, `/api/v1/community/posts/${postId}/comments`, { method: "GET" });
}

export async function POST(request: NextRequest, { params }: { params: Promise<{ postId: string }> }) {
  const { postId } = await params;
  const body = await request.text();
  return proxyToApi(request, `/api/v1/community/posts/${postId}/comments`, {
    method: "POST",
    headers: { "content-type": "application/json" },
    body,
  });
}
