import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function DELETE(request: NextRequest, { params }: { params: Promise<{ postId: string }> }) {
  const { postId } = await params;
  return proxyToApi(request, `/api/v1/community/posts/${postId}`, { method: "DELETE" });
}
