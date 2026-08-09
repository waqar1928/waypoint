import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function POST(request: NextRequest, { params }: { params: Promise<{ reportId: string }> }) {
  const { reportId } = await params;
  return proxyToApi(request, `/api/v1/admin/moderation/${reportId}/remove-content`, { method: "POST" });
}
