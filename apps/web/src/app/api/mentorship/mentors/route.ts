import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function GET(request: NextRequest) {
  const search = request.nextUrl.search;
  return proxyToApi(request, `/api/v1/mentorship/mentors${search}`, { method: "GET" });
}
