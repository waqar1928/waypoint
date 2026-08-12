import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function POST(
  request: NextRequest,
  { params }: { params: Promise<{ notificationId: string }> },
) {
  const { notificationId } = await params;
  return proxyToApi(request, `/api/v1/notifications/${notificationId}/read`, { method: "POST" });
}
