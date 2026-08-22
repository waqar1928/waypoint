import type { MetadataRoute } from "next";

import { SITE_URL } from "@/lib/site-url";

/**
 * Real crawl policy — previously there was none at all (see
 * docs/PRODUCTION_READINESS_AUDIT.md's SEO section), which meant no explicit guidance for search
 * engines on what to index vs. skip. Disallows every private, authenticated surface; only the
 * public marketing/auth pages are crawlable. This is defense-in-depth alongside the per-route
 * `robots: { index: false }` metadata on /app, /admin, /onboarding (a well-behaved crawler
 * respects this file and never requests those paths at all; the metadata tag covers the case
 * where a private URL is linked from somewhere else regardless).
 */
export default function robots(): MetadataRoute.Robots {
  return {
    rules: {
      userAgent: "*",
      allow: "/",
      disallow: ["/app/", "/admin/", "/onboarding", "/api/"],
    },
    sitemap: `${SITE_URL}/sitemap.xml`,
  };
}
