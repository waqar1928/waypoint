import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function PUT(request: NextRequest, { params }: { params: Promise<{ mentorProfileId: string }> }) {
  const { mentorProfileId } = await params;
  const body = await request.text();
  return proxyToApi(request, `/api/v1/admin/mentors/${mentorProfileId}/verification`, {
    method: "PUT",
    headers: { "content-type": "application/json" },
    body,
  });
}
