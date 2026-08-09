import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function POST(request: NextRequest, { params }: { params: Promise<{ helpRequestId: string }> }) {
  const { helpRequestId } = await params;
  return proxyToApi(request, `/api/v1/mentorship/help-requests/${helpRequestId}/close`, { method: "POST" });
}
