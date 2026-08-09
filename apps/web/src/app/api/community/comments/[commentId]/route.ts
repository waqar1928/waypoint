import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function DELETE(request: NextRequest, { params }: { params: Promise<{ commentId: string }> }) {
  const { commentId } = await params;
  return proxyToApi(request, `/api/v1/community/comments/${commentId}`, { method: "DELETE" });
}
