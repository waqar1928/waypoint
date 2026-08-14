import { NextRequest, NextResponse } from "next/server";

/**
 * Generates a per-request nonce and sets the Content-Security-Policy header dynamically, rather
 * than the static header next.config.ts used to set.
 *
 * Why this had to move here: Next.js's App Router emits its own inline <script> tags in the HTML
 * document to stream RSC payloads and hydrate the page (the `self.__next_f.push(...)` scripts
 * visible in any page's source) — this is how the framework itself works, not something this
 * app's code controls. A static `script-src 'self'` with no 'unsafe-inline'/nonce/hash (what
 * next.config.ts previously shipped) blocks those scripts outright: the browser refuses to run
 * them, React never finishes hydrating, and every client-side event handler on the page
 * (including every form's onSubmit) silently never attaches. A form left un-hydrated falls back
 * to the browser's native HTML submission behavior, which defaults to method="get" — this is
 * exactly what caused a real incident where a registration form's password ended up appended to
 * the URL as a query parameter instead of being POSTed. A nonce-based CSP is Next.js's own
 * documented fix for this: generate a random nonce per request, include it in the CSP header,
 * and Next.js automatically applies that same nonce to its own inline scripts so the browser
 * allows them while everything else (any other inline script) is still blocked.
 */
export function middleware(request: NextRequest) {
  const nonce = Buffer.from(crypto.randomUUID()).toString("base64");
  const cspHeader = `
    default-src 'self';
    script-src 'self' 'nonce-${nonce}' 'strict-dynamic';
    style-src 'self' 'unsafe-inline';
    img-src 'self' data: https:;
    font-src 'self' data:;
    connect-src 'self';
    frame-ancestors 'none';
    object-src 'none';
    base-uri 'self';
  `;
  const contentSecurityPolicyHeaderValue = cspHeader.replace(/\s{2,}/g, " ").trim();

  const requestHeaders = new Headers(request.headers);
  requestHeaders.set("x-nonce", nonce);
  requestHeaders.set("Content-Security-Policy", contentSecurityPolicyHeaderValue);

  const response = NextResponse.next({
    request: { headers: requestHeaders },
  });
  response.headers.set("Content-Security-Policy", contentSecurityPolicyHeaderValue);

  return response;
}

export const config = {
  matcher: [
    {
      source: "/((?!api|_next/static|_next/image|favicon.ico).*)",
      missing: [
        { type: "header", key: "next-router-prefetch" },
        { type: "header", key: "purpose", value: "prefetch" },
      ],
    },
  ],
};
