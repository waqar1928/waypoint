import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function PUT(request: NextRequest, { params }: { params: Promise<{ actionId: string }> }) {
  const { actionId } = await params;
  const body = await request.text();
  return proxyToApi(request, `/api/v1/actions/${actionId}`, {
    method: "PUT",
    headers: { "content-type": "application/json" },
    body,
  });
}
