import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function GET(request: NextRequest) {
  return proxyToApi(request, "/api/v1/notifications/push-subscriptions", { method: "GET" });
}

export async function POST(request: NextRequest) {
  const body = await request.text();
  return proxyToApi(request, "/api/v1/notifications/push-subscriptions", {
    method: "POST",
    headers: { "content-type": "application/json" },
    body,
  });
}
