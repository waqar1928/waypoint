import type { NextConfig } from "next";

// Content-Security-Policy is NOT set here anymore — it moved to middleware.ts, the only place a
// per-request nonce can be generated and threaded into the header value. A static CSP here (the
// previous approach) can't include a nonce, and a `script-src 'self'` with no nonce/hash/
// 'unsafe-inline' blocks Next.js's own required inline hydration scripts — see middleware.ts's
// doc comment for what that actually broke in production (registration's password ending up in
// the URL, among other things). The headers below don't need a nonce and stay static/unconditional.
const securityHeaders = [
  { key: "X-Content-Type-Options", value: "nosniff" },
  { key: "X-Frame-Options", value: "DENY" },
  { key: "Referrer-Policy", value: "strict-origin-when-cross-origin" },
  { key: "Permissions-Policy", value: "camera=(), microphone=(), geolocation=()" },
];

const nextConfig: NextConfig = {
  output: "standalone",
  async headers() {
    return [{ source: "/:path*", headers: securityHeaders }];
  },
};

export default nextConfig;
