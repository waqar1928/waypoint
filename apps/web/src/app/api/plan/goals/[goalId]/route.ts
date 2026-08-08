import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function PUT(request: NextRequest, { params }: { params: Promise<{ goalId: string }> }) {
  const { goalId } = await params;
  const body = await request.text();
  return proxyToApi(request, `/api/v1/plan/goals/${goalId}`, {
    method: "PUT",
    headers: { "content-type": "application/json" },
    body,
  });
}
