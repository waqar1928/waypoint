import { NextRequest, NextResponse } from "next/server";
import { API_BASE_URL } from "@/lib/api-config";

/**
 * Forwards a request to the ASP.NET Core API and relays Set-Cookie back to
 * the browser, so the session cookie stays first-party/HttpOnly and the
 * browser never talks to the backend origin directly.
 */
export async function proxyToApi(
  request: NextRequest,
  path: string,
  init?: RequestInit,
) {
  const cookie = request.headers.get("cookie") ?? "";
  const csrfToken = request.headers.get("x-csrf-token");

  let upstream: Response;
  try {
    upstream = await fetch(`${API_BASE_URL}${path}`, {
      ...init,
      headers: {
        ...(init?.headers ?? {}),
        cookie,
        ...(csrfToken ? { "x-csrf-token": csrfToken } : {}),
      },
      redirect: "manual",
    });
  } catch {
    return NextResponse.json(
      {
        type: "https://waypoint.app/errors/upstream-unavailable",
        title: "Waypoint's API is currently unreachable",
        status: 503,
        detail:
          "The backend service is not running or not reachable at " + API_BASE_URL,
      },
      { status: 503 },
    );
  }

  const body = await upstream.text();
  const response = new NextResponse(body || null, {
    status: upstream.status,
    headers: {
      "content-type": upstream.headers.get("content-type") ?? "application/json",
    },
  });

  for (const setCookie of upstream.headers.getSetCookie?.() ?? []) {
    response.headers.append("set-cookie", setCookie);
  }

  return response;
}
