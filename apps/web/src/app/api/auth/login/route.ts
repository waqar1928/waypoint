import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function POST(request: NextRequest) {
  const body = await request.text();
  return proxyToApi(request, "/api/v1/auth/login", {
    method: "POST",
    headers: { "content-type": "application/json" },
    body,
  });
}
