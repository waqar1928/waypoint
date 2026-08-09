import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function GET(request: NextRequest) {
  return proxyToApi(request, "/api/v1/business-idea/validations", { method: "GET" });
}

export async function POST(request: NextRequest) {
  return proxyToApi(request, "/api/v1/business-idea/validations", { method: "POST" });
}
