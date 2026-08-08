import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function POST(request: NextRequest, { params }: { params: Promise<{ milestoneId: string }> }) {
  const { milestoneId } = await params;
  return proxyToApi(request, `/api/v1/milestones/${milestoneId}/achieve`, { method: "POST" });
}
