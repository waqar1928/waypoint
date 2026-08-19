import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function POST(request: NextRequest, { params }: { params: Promise<{ actionId: string }> }) {
  const { actionId } = await params;
  const body = await request.text();
  return proxyToApi(request, `/api/v1/actions/${actionId}/reflection`, {
    method: "POST",
    headers: { "content-type": "application/json" },
    body,
  });
}
