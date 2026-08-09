import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function GET(request: NextRequest, { params }: { params: Promise<{ helpRequestId: string }> }) {
  const { helpRequestId } = await params;
  return proxyToApi(request, `/api/v1/mentorship/help-requests/${helpRequestId}/responses`, { method: "GET" });
}

export async function POST(request: NextRequest, { params }: { params: Promise<{ helpRequestId: string }> }) {
  const { helpRequestId } = await params;
  const body = await request.text();
  return proxyToApi(request, `/api/v1/mentorship/help-requests/${helpRequestId}/responses`, {
    method: "POST",
    headers: { "content-type": "application/json" },
    body,
  });
}
