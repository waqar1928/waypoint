import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function POST(request: NextRequest, { params }: { params: Promise<{ experimentId: string }> }) {
  const { experimentId } = await params;
  const body = await request.text();
  return proxyToApi(request, `/api/v1/experiments/${experimentId}/results`, {
    method: "POST",
    headers: { "content-type": "application/json" },
    body,
  });
}
