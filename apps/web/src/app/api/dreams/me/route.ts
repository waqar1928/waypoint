import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function GET(request: NextRequest) {
  return proxyToApi(request, "/api/v1/dreams/me", { method: "GET" });
}

export async function PUT(request: NextRequest) {
  const body = await request.text();
  return proxyToApi(request, "/api/v1/dreams/me", {
    method: "PUT",
    headers: { "content-type": "application/json" },
    body,
  });
}
