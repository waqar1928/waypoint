import { NextRequest } from "next/server";
import { proxyToApi } from "@/lib/proxy";

export async function POST(request: NextRequest, { params }: { params: Promise<{ actionId: string }> }) {
  const { actionId } = await params;
  return proxyToApi(request, `/api/v1/actions/${actionId}/set-next-best`, { method: "POST" });
}
