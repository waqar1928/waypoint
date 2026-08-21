import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function DELETE(request: NextRequest, { params }: { params: Promise<{ subscriptionId: string }> }) {
  const { subscriptionId } = await params;
  return proxyToApi(request, `/api/v1/notifications/push-subscriptions/${subscriptionId}`, { method: "DELETE" });
}
