import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function DELETE(request: NextRequest) {
  const body = await request.text();
  return proxyToApi(request, "/api/v1/me", {
    method: "DELETE",
    headers: { "content-type": "application/json" },
    body,
  });
}
