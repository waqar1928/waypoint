import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function PUT(request: NextRequest, { params }: { params: Promise<{ experimentId: string }> }) {
  const { experimentId } = await params;
  const body = await request.text();
  return proxyToApi(request, `/api/v1/experiments/${experimentId}/status`, {
    method: "PUT",
    headers: { "content-type": "application/json" },
    body,
  });
}
