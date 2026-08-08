import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function PUT(request: NextRequest, { params }: { params: Promise<{ missionId: string }> }) {
  const { missionId } = await params;
  const body = await request.text();
  return proxyToApi(request, `/api/v1/plan/missions/${missionId}`, {
    method: "PUT",
    headers: { "content-type": "application/json" },
    body,
  });
}
