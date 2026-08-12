import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function POST(request: NextRequest) {
  return proxyToApi(request, "/api/v1/notifications/read-all", { method: "POST" });
}
