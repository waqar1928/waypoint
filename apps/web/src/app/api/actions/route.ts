import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function GET(request: NextRequest) {
  const search = request.nextUrl.search;
  return proxyToApi(request, `/api/v1/actions${search}`, { method: "GET" });
}

export async function POST(request: NextRequest) {
  const body = await request.text();
  return proxyToApi(request, "/api/v1/actions", {
    method: "POST",
    headers: { "content-type": "application/json" },
    body,
  });
}
